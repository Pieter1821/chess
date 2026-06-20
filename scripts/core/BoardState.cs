// The logical chess position: what is on every square and whose turn it is.
// Pure C#, no Godot — this is the source of truth the visuals will mirror.
public sealed class BoardState
{
    private readonly Piece?[,] _squares = new Piece?[8, 8];

    public PieceColor SideToMove { get; private set; } = PieceColor.White;

    public Piece? this[Square s] => _squares[s.File, s.Rank];

    public void Set(Square s, Piece? piece) => _squares[s.File, s.Rank] = piece;

    public void ApplyMove(Move move)
    {
        _squares[move.To.File, move.To.Rank] = _squares[move.From.File, move.From.Rank];
        _squares[move.From.File, move.From.Rank] = null;
        SideToMove = SideToMove == PieceColor.White ? PieceColor.Black : PieceColor.White;
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
