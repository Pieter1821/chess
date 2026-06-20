// The logical chess position: what is on every square and whose turn it is.
// Pure C#, no Godot — this is the source of truth the visuals will mirror.
public sealed class BoardState
{
    private readonly Piece?[,] _squares = new Piece?[8, 8];

    public PieceColor SideToMove { get; private set; } = PieceColor.White;
    public int HalfmoveClock { get; private set; }   // plies since the last capture or pawn move

    public Piece? this[Square s] => _squares[s.File, s.Rank];

    public void Set(Square s, Piece? piece) => _squares[s.File, s.Rank] = piece;

    public void ApplyMove(Move move)
    {
        Piece? mover = _squares[move.From.File, move.From.Rank];
        bool resetsClock = _squares[move.To.File, move.To.Rank] is not null
                           || (mover is Piece p && p.Type == PieceType.Pawn);

        Piece? placed = mover;
        if (move.Promotion is PieceType promo && mover is Piece mp)
            placed = new Piece(promo, mp.Color);

        _squares[move.To.File, move.To.Rank] = placed;
        _squares[move.From.File, move.From.Rank] = null;
        SideToMove = SideToMove == PieceColor.White ? PieceColor.Black : PieceColor.White;
        HalfmoveClock = resetsClock ? 0 : HalfmoveClock + 1;
    }

    // A compact string of the whole position (+ side to move) for repetition detection.
    public string PositionKey()
    {
        var sb = new System.Text.StringBuilder(65);
        for (int r = 0; r < 8; r++)
        for (int f = 0; f < 8; f++)
            sb.Append(_squares[f, r] is Piece pc ? PieceChar(pc) : '.');
        sb.Append(SideToMove == PieceColor.White ? 'w' : 'b');
        return sb.ToString();
    }

    // Forsyth-Edwards Notation: the position string a UCI engine (Stockfish) reads.
    public string ToFen()
    {
        var sb = new System.Text.StringBuilder();
        for (int r = 7; r >= 0; r--)
        {
            int empty = 0;
            for (int f = 0; f < 8; f++)
            {
                if (_squares[f, r] is Piece p)
                {
                    if (empty > 0) { sb.Append(empty); empty = 0; }
                    sb.Append(PieceChar(p));
                }
                else empty++;
            }
            if (empty > 0) sb.Append(empty);
            if (r > 0) sb.Append('/');
        }
        sb.Append(SideToMove == PieceColor.White ? " w " : " b ");
        sb.Append("- - ");           // castling / en passant not tracked yet
        sb.Append(HalfmoveClock);
        sb.Append(" 1");             // fullmove number
        return sb.ToString();
    }

    private static char PieceChar(Piece p)
    {
        char c = p.Type switch
        {
            PieceType.Pawn => 'p', PieceType.Knight => 'n', PieceType.Bishop => 'b',
            PieceType.Rook => 'r', PieceType.Queen => 'q', PieceType.King => 'k', _ => '?',
        };
        return p.Color == PieceColor.White ? char.ToUpper(c) : c;
    }

    public BoardState Clone()
    {
        var copy = new BoardState();
        System.Array.Copy(_squares, copy._squares, _squares.Length);
        copy.SideToMove = SideToMove;
        copy.HalfmoveClock = HalfmoveClock;
        return copy;
    }

    // Returns a NEW board with the move applied (the search uses this to explore safely).
    public BoardState WithMove(Move move)
    {
        BoardState next = Clone();
        next.ApplyMove(move);
        return next;
    }

    public static BoardState CreateStartingPosition()
    {
        var board = new BoardState();
        PieceType[] back =
        {
            PieceType.Rook, PieceType.Knight, PieceType.Bishop, PieceType.Queen,
            PieceType.King, PieceType.Bishop, PieceType.Knight, PieceType.Rook
        };
        for (int file = 0; file < 8; file++)
        {
            board.Set(new Square(file, 0), new Piece(back[file], PieceColor.White));
            board.Set(new Square(file, 1), new Piece(PieceType.Pawn, PieceColor.White));
            board.Set(new Square(file, 6), new Piece(PieceType.Pawn, PieceColor.Black));
            board.Set(new Square(file, 7), new Piece(back[file], PieceColor.Black));
        }
        return board;
    }
}
