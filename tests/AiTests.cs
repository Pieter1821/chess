using Xunit;

public class AiTests
{
    [Fact]
    public void Ai_GrabsAFreeQueen()
    {
        var board = new BoardState();
        board.Set(new Square(0, 0), new Piece(PieceType.King, PieceColor.White));  // a1
        board.Set(new Square(7, 7), new Piece(PieceType.King, PieceColor.Black));  // h8
        board.Set(new Square(3, 0), new Piece(PieceType.Rook, PieceColor.White));  // d1 rook
        board.Set(new Square(3, 5), new Piece(PieceType.Queen, PieceColor.Black)); // d6 queen (undefended)

        Move? move = ChessAi.BestMove(board, PieceColor.White, 2);

        Assert.NotNull(move);
        Assert.Equal(new Square(3, 5), move!.Value.To);   // rook takes the queen
    }

    [Fact]
    public void Ai_ReturnsAMoveFromStart()
    {
        var board = BoardState.CreateStartingPosition();
        Assert.NotNull(ChessAi.BestMove(board, PieceColor.White, 3));
    }
}
