using Xunit;

public class FenTests
{
    [Fact]
    public void StartingPosition_Fen()
    {
        var b = BoardState.CreateStartingPosition();
        Assert.Equal("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", b.ToFen());
    }

    [Fact]
    public void Fen_ReflectsSideToMove()
    {
        var b = BoardState.CreateStartingPosition();
        b.ApplyMove(new Move(new Square(4, 1), new Square(4, 3))); // e2-e4
        Assert.Contains(" b ", b.ToFen());
    }
}
