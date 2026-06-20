using System.Collections.Generic;

public enum GameStatus { Ongoing, Check, Checkmate, Stalemate }

public static class MoveGenerator
{
    private static readonly (int df, int dr)[] KnightOffsets =
        { (1, 2), (2, 1), (2, -1), (1, -2), (-1, -2), (-2, -1), (-2, 1), (-1, 2) };

    private static readonly (int df, int dr)[] KingOffsets =
        { (1, 0), (1, 1), (0, 1), (-1, 1), (-1, 0), (-1, -1), (0, -1), (1, -1) };

    private static readonly (int df, int dr)[] RookDirs = { (1, 0), (-1, 0), (0, 1), (0, -1) };
    private static readonly (int df, int dr)[] BishopDirs = { (1, 1), (1, -1), (-1, 1), (-1, -1) };

    // Fully legal: pseudo-legal moves minus any that leave the mover's own king in check.
    public static List<Move> LegalMoves(BoardState board, Square from)
    {
        var legal = new List<Move>();
        if (board[from] is not Piece piece) return legal;
        foreach (Move m in PseudoLegalMoves(board, from))
            if (!InCheck(board.WithMove(m), piece.Color))
                legal.Add(m);
        return legal;
    }

    public static List<Move> AllMoves(BoardState board, PieceColor color)
        => CollectMoves(board, color, legalOnly: true);

    // Faster, unfiltered set used inside the AI search (king value stands in for legality).
    public static List<Move> AllPseudoMoves(BoardState board, PieceColor color)
        => CollectMoves(board, color, legalOnly: false);

    public static GameStatus Status(BoardState board, PieceColor sideToMove)
    {
        bool inCheck = InCheck(board, sideToMove);
        bool hasMove = AllMoves(board, sideToMove).Count > 0;
        if (!hasMove) return inCheck ? GameStatus.Checkmate : GameStatus.Stalemate;
        return inCheck ? GameStatus.Check : GameStatus.Ongoing;
    }

    public static bool InCheck(BoardState board, PieceColor color)
    {
        Square king = FindKing(board, color);
        return king.IsOnBoard && IsAttacked(board, king, Opposite(color));
    }

    // True for K vs K, K+minor vs K, and same-coloured-bishops positions (can't force mate).
    public static bool IsInsufficientMaterial(BoardState board)
    {
        int knights = 0;
        var bishopColors = new List<int>();
        for (int f = 0; f < 8; f++)
        for (int r = 0; r < 8; r++)
        {
            if (board[new Square(f, r)] is not Piece p) continue;
            switch (p.Type)
            {
                case PieceType.King: break;
                case PieceType.Knight: knights++; break;
                case PieceType.Bishop: bishopColors.Add((f + r) % 2); break;
                default: return false;   // a pawn, rook or queen can deliver mate
            }
        }

        if (knights == 0 && bishopColors.Count == 0) return true;                       // K vs K
        if (knights == 1 && bishopColors.Count == 0) return true;                       // K+N vs K
        if (knights == 0 && bishopColors.TrueForAll(c => c == bishopColors[0])) return true; // same-colour bishops
        return false;
    }

    // ---------- internals ----------

    private static List<Move> CollectMoves(BoardState board, PieceColor color, bool legalOnly)
    {
        var all = new List<Move>();
        for (int f = 0; f < 8; f++)
        for (int r = 0; r < 8; r++)
        {
            var sq = new Square(f, r);
            if (board[sq] is Piece p && p.Color == color)
                all.AddRange(legalOnly ? LegalMoves(board, sq) : PseudoLegalMoves(board, sq));
        }
        return all;
    }

    private static List<Move> PseudoLegalMoves(BoardState board, Square from)
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

    private static void AddStepMoves(BoardState board, Square from, PieceColor me,
                                     (int df, int dr)[] offsets, List<Move> moves)
    {
        foreach (var (df, dr) in offsets)
        {
            var to = new Square(from.File + df, from.Rank + dr);
            if (!to.IsOnBoard) continue;
            if (board[to] is Piece occ && occ.Color == me) continue;
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
                    if (occ.Color != me) moves.Add(new Move(from, to));
                    break;
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
        int promoRank = me == PieceColor.White ? 7 : 0;

        var one = new Square(from.File, from.Rank + dir);
        if (one.IsOnBoard && board[one] is null)
        {
            AddPawnMove(moves, from, one, promoRank);
            var two = new Square(from.File, from.Rank + 2 * dir);
            if (from.Rank == startRank && board[two] is null)
                moves.Add(new Move(from, two));
        }

        foreach (int df in new[] { -1, 1 })
        {
            var cap = new Square(from.File + df, from.Rank + dir);
            if (cap.IsOnBoard && board[cap] is Piece occ && occ.Color != me)
                AddPawnMove(moves, from, cap, promoRank);
        }
    }

    // Reaching the last rank promotes: emit one move per promotion choice.
    private static void AddPawnMove(List<Move> moves, Square from, Square to, int promoRank)
    {
        if (to.Rank == promoRank)
        {
            moves.Add(new Move(from, to, PieceType.Queen));
            moves.Add(new Move(from, to, PieceType.Rook));
            moves.Add(new Move(from, to, PieceType.Bishop));
            moves.Add(new Move(from, to, PieceType.Knight));
        }
        else
        {
            moves.Add(new Move(from, to));
        }
    }

    // Is `sq` attacked by any piece of color `by`?
    public static bool IsAttacked(BoardState board, Square sq, PieceColor by)
    {
        // Pawn: a `by` pawn attacks diagonally forward, so it sits one rank "behind" sq.
        int dir = by == PieceColor.White ? 1 : -1;
        foreach (int df in new[] { -1, 1 })
        {
            var p = new Square(sq.File + df, sq.Rank - dir);
            if (p.IsOnBoard && board[p] is Piece pc && pc.Color == by && pc.Type == PieceType.Pawn)
                return true;
        }

        if (HasAttackerAt(board, sq, by, KnightOffsets, PieceType.Knight)) return true;
        if (HasAttackerAt(board, sq, by, KingOffsets, PieceType.King)) return true;
        if (SlideAttack(board, sq, by, RookDirs, PieceType.Rook)) return true;
        if (SlideAttack(board, sq, by, BishopDirs, PieceType.Bishop)) return true;
        return false;
    }

    private static bool HasAttackerAt(BoardState board, Square sq, PieceColor by,
                                      (int df, int dr)[] offsets, PieceType type)
    {
        foreach (var (df, dr) in offsets)
        {
            var s = new Square(sq.File + df, sq.Rank + dr);
            if (s.IsOnBoard && board[s] is Piece p && p.Color == by && p.Type == type)
                return true;
        }
        return false;
    }

    private static bool SlideAttack(BoardState board, Square sq, PieceColor by,
                                    (int df, int dr)[] dirs, PieceType sliderType)
    {
        foreach (var (df, dr) in dirs)
        {
            var s = new Square(sq.File + df, sq.Rank + dr);
            while (s.IsOnBoard)
            {
                if (board[s] is Piece occ)
                {
                    if (occ.Color == by && (occ.Type == sliderType || occ.Type == PieceType.Queen))
                        return true;
                    break;
                }
                s = new Square(s.File + df, s.Rank + dr);
            }
        }
        return false;
    }

    private static Square FindKing(BoardState board, PieceColor color)
    {
        for (int f = 0; f < 8; f++)
        for (int r = 0; r < 8; r++)
        {
            var sq = new Square(f, r);
            if (board[sq] is Piece p && p.Type == PieceType.King && p.Color == color)
                return sq;
        }
        return new Square(-1, -1);
    }

    private static PieceColor Opposite(PieceColor c) =>
        c == PieceColor.White ? PieceColor.Black : PieceColor.White;
}
