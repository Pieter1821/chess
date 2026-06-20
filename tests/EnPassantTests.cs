using Xunit;

public class EnPassantTests
{
    [Fact]
    public void Target_SetAfterDoublePush()
    {
        var b = BoardState.CreateStartingPosition();
        b.ApplyMove(new Move(new Square(4, 1), new Square(4, 3))); // e2-e4
        Assert.Equal(new Square(4, 2), b.EnPassantTarget);          // e3
    }

    private static BoardState EnPassantSetup()
    {
        var b = new BoardState();
        b.Set(new Square(0, 0), new Piece(PieceType.King, PieceColor.White));
        b.Set(new Square(7, 7), new Piece(PieceType.King, PieceColor.Black));
        b.Set(new Square(4, 4), new Piece(PieceType.Pawn, PieceColor.White)); // e5
        b.Set(new Square(3, 6), new Piece(PieceType.Pawn, PieceColor.Black)); // d7
        b.ApplyMove(new Move(new Square(3, 6), new Square(3, 4)));            // d7-d5 -> ep target d6
        return b;
    }

    [Fact]
    public void Capture_IsGenerated()
    {
        var b = EnPassantSetup();
        var moves = MoveGenerator.LegalMoves(b, new Square(4, 4)); // white pawn e5
        Assert.Contains(moves, m => m.To == new Square(3, 5));     // exd6 e.p.
    }

    [Fact]
    public void Capture_RemovesTheEnemyPawn()
    {
        var b = EnPassantSetup();
        b.ApplyMove(new Move(new Square(4, 4), new Square(3, 5))); // exd6 e.p.
        Assert.Equal(PieceType.Pawn, b[new Square(3, 5)]!.Value.Type); // white pawn on d6
        Assert.Null(b[new Square(3, 4)]);                             // captured d5 pawn is gone
    }
}
