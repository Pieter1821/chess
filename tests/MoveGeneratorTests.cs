using Xunit;

public class MoveGeneratorTests
{
    [Fact]
    public void StartingPosition_WhiteHas20Moves()
    {
        var board = BoardState.CreateStartingPosition();
        Assert.Equal(20, MoveGenerator.AllMoves(board, PieceColor.White).Count);
    }

    [Fact]
    public void StartingPosition_BlackHas20Moves()
    {
        var board = BoardState.CreateStartingPosition();
        Assert.Equal(20, MoveGenerator.AllMoves(board, PieceColor.Black).Count);
    }

    [Fact]
    public void Knight_FromB1_HasTwoMoves()
    {
        var board = BoardState.CreateStartingPosition();
        var moves = MoveGenerator.LegalMoves(board, new Square(1, 0)); // b1
        Assert.Equal(2, moves.Count);
    }

    [Fact]
    public void Bishop_AtStart_IsBlocked()
    {
        var board = BoardState.CreateStartingPosition();
        var moves = MoveGenerator.LegalMoves(board, new Square(2, 0)); // c1
        Assert.Empty(moves);
    }

    [Fact]
    public void Rook_OnEmptyBoard_Has14Moves()
    {
        var board = new BoardState();
        board.Set(new Square(3, 3), new Piece(PieceType.Rook, PieceColor.White)); // d4
        var moves = MoveGenerator.LegalMoves(board, new Square(3, 3));
        Assert.Equal(14, moves.Count);
    }

    [Fact]
    public void Pawn_OnStart_CanPushOneOrTwo()
    {
        var board = BoardState.CreateStartingPosition();
        var moves = MoveGenerator.LegalMoves(board, new Square(4, 1)); // e2
        Assert.Equal(2, moves.Count);
    }
}
