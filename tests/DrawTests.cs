using Xunit;

public class DrawTests
{
    [Fact]
    public void KingVsKing_IsInsufficient()
    {
        var b = new BoardState();
        b.Set(new Square(0, 0), new Piece(PieceType.King, PieceColor.White));
        b.Set(new Square(7, 7), new Piece(PieceType.King, PieceColor.Black));
        Assert.True(MoveGenerator.IsInsufficientMaterial(b));
    }

    [Fact]
    public void KingKnightVsKing_IsInsufficient()
    {
        var b = new BoardState();
        b.Set(new Square(0, 0), new Piece(PieceType.King, PieceColor.White));
        b.Set(new Square(7, 7), new Piece(PieceType.King, PieceColor.Black));
        b.Set(new Square(1, 0), new Piece(PieceType.Knight, PieceColor.White));
        Assert.True(MoveGenerator.IsInsufficientMaterial(b));
    }

    [Fact]
    public void KingRookVsKing_IsSufficient()
    {
        var b = new BoardState();
        b.Set(new Square(0, 0), new Piece(PieceType.King, PieceColor.White));
        b.Set(new Square(7, 7), new Piece(PieceType.King, PieceColor.Black));
        b.Set(new Square(1, 0), new Piece(PieceType.Rook, PieceColor.White));
        Assert.False(MoveGenerator.IsInsufficientMaterial(b));
    }

    [Fact]
    public void HalfmoveClock_ResetsOnPawnMove()
    {
        var b = BoardState.CreateStartingPosition();
        b.ApplyMove(new Move(new Square(1, 0), new Square(2, 2))); // knight move -> clock = 1
        Assert.Equal(1, b.HalfmoveClock);
        b.ApplyMove(new Move(new Square(4, 6), new Square(4, 4))); // pawn move -> clock resets
        Assert.Equal(0, b.HalfmoveClock);
    }
}
