// The logical chess position: what is on every square and whose turn it is.
// Pure C#, no Godot — this is the source of truth the visuals mirror.
public sealed class BoardState
{
    private readonly Piece?[,] _squares = new Piece?[8, 8];

    public PieceColor SideToMove { get; private set; } = PieceColor.White;
    public int HalfmoveClock { get; private set; }   // plies since the last capture or pawn move

    // Castling rights.
    public bool WhiteOO { get; private set; } = true;   // white kingside
    public bool WhiteOOO { get; private set; } = true;  // white queenside
    public bool BlackOO { get; private set; } = true;
    public bool BlackOOO { get; private set; } = true;

    // Square an enemy pawn may capture onto via en passant (set the move after a double push).
    public Square? EnPassantTarget { get; private set; }

    public Piece? this[Square s] => _squares[s.File, s.Rank];

    public void Set(Square s, Piece? piece) => _squares[s.File, s.Rank] = piece;

    public void ApplyMove(Move move)
    {
        Square from = move.From, to = move.To;
        Piece? mover = _squares[from.File, from.Rank];
        bool isPawn = mover is Piece p0 && p0.Type == PieceType.Pawn;
        bool isKing = mover is Piece k0 && k0.Type == PieceType.King;
        bool normalCapture = _squares[to.File, to.Rank] is not null;
        bool enPassant = isPawn && EnPassantTarget is Square ep && to == ep
                         && from.File != to.File && !normalCapture;

        UpdateCastlingRights(from, to);

        // Move the piece (handle promotion).
        Piece? placed = mover;
        if (move.Promotion is PieceType promo && mover is Piece mp)
            placed = new Piece(promo, mp.Color);
        _squares[to.File, to.Rank] = placed;
        _squares[from.File, from.Rank] = null;

        // Castling: relocate the rook to the other side of the king.
        if (isKing && System.Math.Abs(to.File - from.File) == 2)
        {
            int rank = from.Rank;
            if (to.File == 6) { _squares[5, rank] = _squares[7, rank]; _squares[7, rank] = null; }      // kingside
            else if (to.File == 2) { _squares[3, rank] = _squares[0, rank]; _squares[0, rank] = null; } // queenside
        }

        // En passant: remove the captured pawn (it sits beside the destination).
        if (enPassant)
            _squares[to.File, from.Rank] = null;

        // New en passant target — only right after a two-square pawn push.
        EnPassantTarget = (isPawn && System.Math.Abs(to.Rank - from.Rank) == 2)
            ? new Square(from.File, (from.Rank + to.Rank) / 2)
            : (Square?)null;

        SideToMove = SideToMove == PieceColor.White ? PieceColor.Black : PieceColor.White;
        HalfmoveClock = (normalCapture || enPassant || isPawn) ? 0 : HalfmoveClock + 1;
    }

    private void UpdateCastlingRights(Square from, Square to)
    {
        foreach (Square s in new[] { from, to })
        {
            if (s.File == 4 && s.Rank == 0) { WhiteOO = false; WhiteOOO = false; }
            else if (s.File == 4 && s.Rank == 7) { BlackOO = false; BlackOOO = false; }
            else if (s.File == 0 && s.Rank == 0) WhiteOOO = false;
            else if (s.File == 7 && s.Rank == 0) WhiteOO = false;
            else if (s.File == 0 && s.Rank == 7) BlackOOO = false;
            else if (s.File == 7 && s.Rank == 7) BlackOO = false;
        }
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

        string castle = "";
        if (WhiteOO) castle += "K";
        if (WhiteOOO) castle += "Q";
        if (BlackOO) castle += "k";
        if (BlackOOO) castle += "q";
        sb.Append(castle.Length == 0 ? "-" : castle);

        sb.Append(' ');
        sb.Append(EnPassantTarget is Square epSq ? epSq.ToString() : "-");
        sb.Append(' ');
        sb.Append(HalfmoveClock);
        sb.Append(" 1");
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
        copy.WhiteOO = WhiteOO;
        copy.WhiteOOO = WhiteOOO;
        copy.BlackOO = BlackOO;
        copy.BlackOOO = BlackOOO;
        copy.EnPassantTarget = EnPassantTarget;
        return copy;
    }

    // Returns a NEW board with the move applied (the search/notation uses this to explore safely).
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
