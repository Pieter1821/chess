using Xunit;

public class CastlingTests
{
    private static BoardState KingAndRook()
    {
        var b = new BoardState();
        b.Set(new Square(4, 0), new Piece(PieceType.King, PieceColor.White)); // e1
        b.Set(new Square(7, 0), new Piece(PieceType.Rook, PieceColor.White)); // h1
        b.Set(new Square(4, 7), new Piece(PieceType.King, PieceColor.Black)); // e8
        return b;
    }

    [Fact]
    public void White_CanCastleKingside_WhenClear()
    {
        var b = KingAndRook();
        var moves = MoveGenerator.LegalMoves(b, new Square(4, 0));
        Assert.Contains(moves, m => m.To == new Square(6, 0)); // king to g1
    }

    [Fact]
    public void Castling_MovesTheRook()
    {
        var b = KingAndRook();
        b.ApplyMove(new Move(new Square(4, 0), new Square(6, 0))); // O-O

        Assert.Equal(PieceType.King, b[new Square(6, 0)]!.Value.Type); // king on g1
        Assert.Equal(PieceType.Rook, b[new Square(5, 0)]!.Value.Type); // rook on f1
        Assert.Null(b[new Square(7, 0)]);                              // h1 empty
    }

    [Fact]
    public void CannotCastle_ThroughCheck()
    {
        var b = KingAndRook();
        b.Set(new Square(5, 7), new Piece(PieceType.Rook, PieceColor.Black)); // f8 rook attacks f1
        var moves = MoveGenerator.LegalMoves(b, new Square(4, 0));
        Assert.DoesNotContain(moves, m => m.To == new Square(6, 0));
    }

    [Fact]
    public void CastlingRight_LostAfterKingMoves()
    {
        var b = KingAndRook();
        b.ApplyMove(new Move(new Square(4, 0), new Square(4, 1))); // king steps to e2
        Assert.False(b.WhiteOO);
    }
}
