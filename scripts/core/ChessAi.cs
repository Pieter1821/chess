using System;
using System.Collections.Generic;

// A minimax (with alpha-beta pruning) chess AI. Pure C#, no Godot, so it's testable.
// Evaluation is material-only for now; the king's huge value stands in for check
// rules until we build them.
public static class ChessAi
{
    private const int KingValue = 10000;

    public static Move? BestMove(BoardState board, PieceColor color, int depth)
    {
        List<Move> moves = MoveGenerator.AllMoves(board, color);
        if (moves.Count == 0) return null;

        bool maximizing = color == PieceColor.White;
        Move best = moves[0];
        int bestEval = maximizing ? int.MinValue : int.MaxValue;
        int alpha = int.MinValue, beta = int.MaxValue;

        foreach (Move m in moves)
        {
            int eval = Search(board.WithMove(m), depth - 1, alpha, beta, Opposite(color));
            if (maximizing)
            {
                if (eval > bestEval) { bestEval = eval; best = m; }
                alpha = Math.Max(alpha, eval);
            }
            else
            {
                if (eval < bestEval) { bestEval = eval; best = m; }
                beta = Math.Min(beta, eval);
            }
            if (beta <= alpha) break;
        }
        return best;
    }

    private static int Search(BoardState board, int depth, int alpha, int beta, PieceColor toMove)
    {
        if (depth == 0) return Evaluate(board);

        // Use the fast unfiltered moves inside the search; the king's huge value keeps it honest.
        List<Move> moves = MoveGenerator.AllPseudoMoves(board, toMove);
        if (moves.Count == 0) return Evaluate(board);

        bool maximizing = toMove == PieceColor.White;
        int value = maximizing ? int.MinValue : int.MaxValue;

        foreach (Move m in moves)
        {
            int eval = Search(board.WithMove(m), depth - 1, alpha, beta, Opposite(toMove));
            if (maximizing)
            {
                value = Math.Max(value, eval);
                alpha = Math.Max(alpha, value);
            }
            else
            {
                value = Math.Min(value, eval);
                beta = Math.Min(beta, value);
            }
            if (beta <= alpha) break;   // prune
        }
        return value;
    }

    // Positive = good for White, negative = good for Black.
    private static int Evaluate(BoardState board)
    {
        int score = 0;
        for (int f = 0; f < 8; f++)
        for (int r = 0; r < 8; r++)
        {
            if (board[new Square(f, r)] is Piece p)
                score += p.Color == PieceColor.White ? Value(p.Type) : -Value(p.Type);
        }
        return score;
    }

    private static int Value(PieceType type) => type switch
    {
        PieceType.Pawn => 100,
        PieceType.Knight => 320,
        PieceType.Bishop => 330,
        PieceType.Rook => 500,
        PieceType.Queen => 900,
        PieceType.King => KingValue,
        _ => 0,
    };

    private static PieceColor Opposite(PieceColor c) =>
        c == PieceColor.White ? PieceColor.Black : PieceColor.White;
}
