using System.Collections.Generic;

// Converts a move into Standard Algebraic Notation (SAN) and a plain-English description.
public static class Notation
{
    public static string ToSan(BoardState before, Move move)
    {
        if (before[move.From] is not Piece piece)
            return $"{move.From}{move.To}";

        string body;
        if (IsCastle(piece, move))
        {
            body = move.To.File == 6 ? "O-O" : "O-O-O";
        }
        else
        {
            bool isCapture = IsCapture(before, move, piece);
            if (piece.Type == PieceType.Pawn)
                body = isCapture ? $"{FileChar(move.From.File)}x{move.To}" : $"{move.To}";
            else
                body = $"{Letter(piece.Type)}{Disambiguation(before, move, piece)}{(isCapture ? "x" : "")}{move.To}";

            if (move.Promotion is PieceType promo) body += $"={Letter(promo)}";
        }

        return body + Suffix(before, move, piece, mate: "#", check: "+");
    }

    // A plain-English description for players who don't read chess notation.
    public static string ToFriendly(BoardState before, Move move)
    {
        if (before[move.From] is not Piece piece) return $"{move.From} to {move.To}";

        string text;
        if (IsCastle(piece, move))
        {
            text = move.To.File == 6 ? "Castles kingside" : "Castles queenside";
        }
        else
        {
            string verb = IsCapture(before, move, piece) ? "takes" : "to";
            text = $"{Name(piece.Type)} {move.From} {verb} {move.To}";
            if (move.Promotion is PieceType promo) text += $", promotes to {Name(promo)}";
        }

        return text + Suffix(before, move, piece, mate: " checkmate", check: " check");
    }

    private static bool IsCastle(Piece piece, Move move)
        => piece.Type == PieceType.King && System.Math.Abs(move.To.File - move.From.File) == 2;

    private static bool IsCapture(BoardState before, Move move, Piece piece)
        => before[move.To] is Piece
           || (piece.Type == PieceType.Pawn && move.From.File != move.To.File);   // includes en passant

    private static string Suffix(BoardState before, Move move, Piece piece, string mate, string check)
    {
        BoardState after = before.WithMove(move);
        PieceColor opponent = piece.Color == PieceColor.White ? PieceColor.Black : PieceColor.White;
        return MoveGenerator.Status(after, opponent) switch
        {
            GameStatus.Checkmate => mate,
            GameStatus.Check => check,
            _ => "",
        };
    }

    // If another same-type piece could also move to the destination, add just enough
    // of the source square (file, else rank, else both) to make it unambiguous.
    private static string Disambiguation(BoardState board, Move move, Piece piece)
    {
        var others = new List<Square>();
        for (int f = 0; f < 8; f++)
        for (int r = 0; r < 8; r++)
        {
            var sq = new Square(f, r);
            if (sq == move.From) continue;
            if (board[sq] is Piece p && p.Type == piece.Type && p.Color == piece.Color
                && MoveGenerator.LegalMoves(board, sq).Exists(m => m.To == move.To))
                others.Add(sq);
        }

        if (others.Count == 0) return "";
        bool sameFile = others.Exists(s => s.File == move.From.File);
        bool sameRank = others.Exists(s => s.Rank == move.From.Rank);
        if (!sameFile) return $"{FileChar(move.From.File)}";
        if (!sameRank) return $"{move.From.Rank + 1}";
        return $"{FileChar(move.From.File)}{move.From.Rank + 1}";
    }

    private static char FileChar(int file) => (char)('a' + file);

    private static string Letter(PieceType type) => type switch
    {
        PieceType.Knight => "N",
        PieceType.Bishop => "B",
        PieceType.Rook => "R",
        PieceType.Queen => "Q",
        PieceType.King => "K",
        _ => "",
    };

    private static string Name(PieceType type) => type switch
    {
        PieceType.Pawn => "Pawn",
        PieceType.Knight => "Knight",
        PieceType.Bishop => "Bishop",
        PieceType.Rook => "Rook",
        PieceType.Queen => "Queen",
        PieceType.King => "King",
        _ => "",
    };
}
