using System.Collections.Generic;

// Produces pseudo-legal moves (correct motion + blocking + captures).
// "Leaves own king in check" filtering is added in the check phase.
public static class MoveGenerator
{
    private static readonly (int df, int dr)[] KnightOffsets =
        { (1, 2), (2, 1), (2, -1), (1, -2), (-1, -2), (-2, -1), (-2, 1), (-1, 2) };

    private static readonly (int df, int dr)[] KingOffsets =
        { (1, 0), (1, 1), (0, 1), (-1, 1), (-1, 0), (-1, -1), (0, -1), (1, -1) };

    private static readonly (int df, int dr)[] RookDirs = { (1, 0), (-1, 0), (0, 1), (0, -1) };
    private static readonly (int df, int dr)[] BishopDirs = { (1, 1), (1, -1), (-1, 1), (-1, -1) };

    public static List<Move> LegalMoves(BoardState board, Square from)
    {
        var moves = new List<Move>();
        if (board[from] is not Piece piece) return moves;

        switch (piece.Type)
        {
            case PieceType.Pawn:   AddPawnMoves(board, from, piece.Color, moves); break;
            case PieceType.Knight: AddStepMoves(board, from, piece.Color, KnightOffsets, moves); break;
            case PieceType.King:   AddStepMoves(board, from, piece.Color, KingOffsets, moves); break;
            case PieceType.Bishop: AddSlideMoves(board, from, piece.Color, BishopDirs, moves); break;
            case PieceType.Rook:   AddSlideMoves(board, from, piece.Color, RookDirs, moves); break;
            case PieceType.Queen:
                AddSlideMoves(board, from, piece.Color, RookDirs, moves);
                AddSlideMoves(board, from, piece.Color, BishopDirs, moves);
                break;
        }
        return moves;
    }

    public static List<Move> AllMoves(BoardState board, PieceColor color)
    {
        var all = new List<Move>();
        for (int f = 0; f < 8; f++)
        for (int r = 0; r < 8; r++)
        {
            var sq = new Square(f, r);
            if (board[sq] is Piece p && p.Color == color)
                all.AddRange(LegalMoves(board, sq));
        }
        return all;
    }

    private static void AddStepMoves(BoardState board, Square from, PieceColor me,
                                     (int df, int dr)[] offsets, List<Move> moves)
    {
        foreach (var (df, dr) in offsets)
        {
            var to = new Square(from.File + df, from.Rank + dr);
            if (!to.IsOnBoard) continue;
            if (board[to] is Piece occ && occ.Color == me) continue; // own piece blocks
            moves.Add(new Move(from, to));
        }
    }

    private static void AddSlideMoves(BoardState board, Square from, PieceColor me,
                                      (int df, int dr)[] dirs, List<Move> moves)
    {
        foreach (var (df, dr) in dirs)
        {
            var to = new Square(from.File + df, from.Rank + dr);
            while (to.IsOnBoard)
            {
                if (board[to] is Piece occ)
                {
                    if (occ.Color != me) moves.Add(new Move(from, to)); // capture
                    break;                                              // blocked either way
                }
                moves.Add(new Move(from, to));
                to = new Square(to.File + df, to.Rank + dr);
            }
        }
    }

    private static void AddPawnMoves(BoardState board, Square from, PieceColor me, List<Move> moves)
    {
        int dir = me == PieceColor.White ? 1 : -1;
        int startRank = me == PieceColor.White ? 1 : 6;

        var one = new Square(from.File, from.Rank + dir);
        if (one.IsOnBoard && board[one] is null)
        {
            moves.Add(new Move(from, one));
            var two = new Square(from.File, from.Rank + 2 * dir);
            if (from.Rank == startRank && board[two] is null)
                moves.Add(new Move(from, two));
        }

        foreach (int df in new[] { -1, 1 })
        {
            var cap = new Square(from.File + df, from.Rank + dir);
            if (cap.IsOnBoard && board[cap] is Piece occ && occ.Color != me)
                moves.Add(new Move(from, cap));
        }
    }
}
