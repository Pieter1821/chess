using Xunit;

public class NotationTests
{
    [Fact]
    public void PawnPush()
    {
        var b = BoardState.CreateStartingPosition();
        Assert.Equal("e4", Notation.ToSan(b, new Move(new Square(4, 1), new Square(4, 3))));
    }

    [Fact]
    public void KnightDevelop()
    {
        var b = BoardState.CreateStartingPosition();
        Assert.Equal("Nf3", Notation.ToSan(b, new Move(new Square(6, 0), new Square(5, 2))));
    }

    [Fact]
    public void PawnCapture()
    {
        var b = new BoardState();
        b.Set(new Square(0, 0), new Piece(PieceType.King, PieceColor.White));
        b.Set(new Square(7, 7), new Piece(PieceType.King, PieceColor.Black));
        b.Set(new Square(4, 3), new Piece(PieceType.Pawn, PieceColor.White)); // e4
        b.Set(new Square(3, 4), new Piece(PieceType.Pawn, PieceColor.Black)); // d5
        Assert.Equal("exd5", Notation.ToSan(b, new Move(new Square(4, 3), new Square(3, 4))));
    }

    [Fact]
    public void RookDisambiguation()
    {
        var b = new BoardState();
        b.Set(new Square(3, 3), new Piece(PieceType.King, PieceColor.White)); // d4
        b.Set(new Square(4, 7), new Piece(PieceType.King, PieceColor.Black));
        b.Set(new Square(0, 0), new Piece(PieceType.Rook, PieceColor.White)); // a1
        b.Set(new Square(7, 0), new Piece(PieceType.Rook, PieceColor.White)); // h1
        Assert.Equal("Rad1", Notation.ToSan(b, new Move(new Square(0, 0), new Square(3, 0))));
    }

    [Fact]
    public void Friendly_PlainEnglish()
    {
        var b = BoardState.CreateStartingPosition();
        Assert.Equal("Pawn e2 to e4", Notation.ToFriendly(b, new Move(new Square(4, 1), new Square(4, 3))));
    }

    [Fact]
    public void CheckSuffix()
    {
        var b = new BoardState();
        b.Set(new Square(0, 0), new Piece(PieceType.King, PieceColor.White));  // a1
        b.Set(new Square(3, 7), new Piece(PieceType.King, PieceColor.Black));  // d8
        b.Set(new Square(3, 0), new Piece(PieceType.Queen, PieceColor.White)); // d1
        Assert.Equal("Qd7+", Notation.ToSan(b, new Move(new Square(3, 0), new Square(3, 6))));
    }
}
