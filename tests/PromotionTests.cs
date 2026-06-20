using Xunit;

public class PromotionTests
{
    [Fact]
    public void Pawn_OnSeventh_GeneratesFourPromotions()
    {
        var b = new BoardState();
        b.Set(new Square(0, 0), new Piece(PieceType.King, PieceColor.White));
        b.Set(new Square(7, 7), new Piece(PieceType.King, PieceColor.Black));
        b.Set(new Square(4, 6), new Piece(PieceType.Pawn, PieceColor.White)); // e7

        var moves = MoveGenerator.LegalMoves(b, new Square(4, 6));

        Assert.Equal(4, moves.Count);
        Assert.Contains(moves, m => m.Promotion == PieceType.Queen && m.To == new Square(4, 7));
        Assert.Contains(moves, m => m.Promotion == PieceType.Knight);
    }

    [Fact]
    public void ApplyMove_PlacesPromotedPiece()
    {
        var b = new BoardState();
        b.Set(new Square(4, 6), new Piece(PieceType.Pawn, PieceColor.White));
        b.ApplyMove(new Move(new Square(4, 6), new Square(4, 7), PieceType.Queen));

        Assert.Equal(PieceType.Queen, b[new Square(4, 7)]!.Value.Type);
        Assert.Equal(PieceColor.White, b[new Square(4, 7)]!.Value.Color);
    }
}
