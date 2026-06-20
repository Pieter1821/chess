using Xunit;

public class CheckTests
{
    [Fact]
    public void PinnedPiece_CannotExposeItsKing()
    {
        // White king a1, white rook a2, black rook a8: the rook is pinned to the a-file.
        var board = new BoardState();
        board.Set(new Square(0, 0), new Piece(PieceType.King, PieceColor.White));
        board.Set(new Square(0, 1), new Piece(PieceType.Rook, PieceColor.White));
        board.Set(new Square(0, 7), new Piece(PieceType.Rook, PieceColor.Black));

        var moves = MoveGenerator.LegalMoves(board, new Square(0, 1));

        Assert.NotEmpty(moves);
        Assert.All(moves, m => Assert.Equal(0, m.To.File)); // every legal move stays on the a-file
    }

    [Fact]
    public void DetectsCheckmate()
    {
        // Black king h8, white queen h7 (defended by white king g6): mate.
        var board = new BoardState();
        board.Set(new Square(7, 7), new Piece(PieceType.King, PieceColor.Black));
        board.Set(new Square(7, 6), new Piece(PieceType.Queen, PieceColor.White));
        board.Set(new Square(6, 5), new Piece(PieceType.King, PieceColor.White));

        Assert.Equal(GameStatus.Checkmate, MoveGenerator.Status(board, PieceColor.Black));
    }

    [Fact]
    public void DetectsStalemate()
    {
        // Black king a8, white queen b6, white king c6: black not in check but has no move.
        var board = new BoardState();
        board.Set(new Square(0, 7), new Piece(PieceType.King, PieceColor.Black));
        board.Set(new Square(1, 5), new Piece(PieceType.Queen, PieceColor.White));
        board.Set(new Square(2, 5), new Piece(PieceType.King, PieceColor.White));

        Assert.Equal(GameStatus.Stalemate, MoveGenerator.Status(board, PieceColor.Black));
    }
}
