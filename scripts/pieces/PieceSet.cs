using System.Collections.Generic;
using Godot;

public partial class PieceSet : Node3D
{
    [Export] public float PieceScale = 18.0f;
    [Export] public float SurfaceY = 0.1f;
    [Export] public float LiftHeight = 0.35f;
    [Export] public float MoveTime = 0.25f;
    [Export] public float LiftTime = 0.12f;
    [Export] public float AiThinkTime = 0.4f;
    [Export] public int AiDepth = 3;

    private static readonly PieceType[] BackRank =
    {
        PieceType.Rook, PieceType.Knight, PieceType.Bishop, PieceType.Queen,
        PieceType.King, PieceType.Bishop, PieceType.Knight, PieceType.Rook
    };

    private readonly Node3D?[,] _pieces = new Node3D?[8, 8];
    private BoardState _state = null!;
    private Board _boardView = null!;
    private Hud _hud = null!;

    private Node3D? _selected;
    private Square _selSquare;
    private readonly HashSet<Square> _legalTargets = new();

    private bool _vsComputer;
    private PieceColor _aiColor;
    private bool _gameOver;

    private int _capturedWhite, _capturedBlack;

    public override void _Ready()
    {
        _boardView = GetNode<Board>("../Board");
        _hud = GetNode<Hud>("../Hud");
        _state = BoardState.CreateStartingPosition();

        for (int file = 0; file < 8; file++)
        {
            SpawnPiece(BackRank[file], PieceColor.White, file, 0);
            SpawnPiece(PieceType.Pawn,  PieceColor.White, file, 1);
            SpawnPiece(PieceType.Pawn,  PieceColor.Black, file, 6);
            SpawnPiece(BackRank[file], PieceColor.Black, file, 7);
        }
    }

    public void StartVsComputer(PieceColor playerColor)
    {
        _vsComputer = true;
        _aiColor = playerColor == PieceColor.White ? PieceColor.Black : PieceColor.White;
        _hud.StartClock();
        _hud.SetStatus($"{_state.SideToMove} to move");
        MaybeTriggerAi();   // computer moves first if it is White
    }

    public void HandleClick(int file, int rank)
    {
        if (_gameOver) return;
        if (_vsComputer && _state.SideToMove == _aiColor) return;

        var sq = new Square(file, rank);

        if (_selected == null) { TrySelect(sq); return; }
        if (sq == _selSquare) { Deselect(); return; }

        if (_legalTargets.Contains(sq))
        {
            PlayerMove(_selSquare, sq);
            return;
        }

        Deselect();
        TrySelect(sq);
    }

    private void TrySelect(Square sq)
    {
        if (_state[sq] is not Piece piece || piece.Color != _state.SideToMove) return;

        _selected = _pieces[sq.File, sq.Rank];
        _selSquare = sq;
        AnimateTo(_selected!, new Vector3(sq.File, SurfaceY + LiftHeight, sq.Rank), LiftTime);

        _legalTargets.Clear();
        _boardView.ClearHighlights();
        _boardView.SetHighlight(sq.File, sq.Rank, Board.HighlightKind.Selected);
        foreach (Move m in MoveGenerator.LegalMoves(_state, sq))
        {
            _legalTargets.Add(m.To);
            Board.HighlightKind kind = _state[m.To] is Piece
                ? Board.HighlightKind.Capture
                : Board.HighlightKind.Move;
            _boardView.SetHighlight(m.To.File, m.To.Rank, kind);
        }
    }

    private void Deselect()
    {
        if (_selected != null)
            AnimateTo(_selected, new Vector3(_selSquare.File, SurfaceY, _selSquare.Rank), LiftTime);
        _selected = null;
        _legalTargets.Clear();
        _boardView.ClearHighlights();
    }

    private void PlayerMove(Square from, Square to)
    {
        _selected = null;
        _legalTargets.Clear();
        _boardView.ClearHighlights();

        ExecuteMove(from, to);
        _hud.IncrementMoves();
        if (AfterMove()) MaybeTriggerAi();
    }

    private void ExecuteMove(Square from, Square to)
    {
        Node3D moving = _pieces[from.File, from.Rank]!;

        Node3D? occupant = _pieces[to.File, to.Rank];
        if (occupant != null && _state[to] is Piece captured)
            SendToTray(occupant, captured.Color);

        _pieces[to.File, to.Rank] = moving;
        _pieces[from.File, from.Rank] = null;
        _state.ApplyMove(new Move(from, to));

        AnimateTo(moving, new Vector3(to.File, SurfaceY, to.Rank), MoveTime);
    }

    // Updates the HUD for the new side to move; returns false if the game has ended.
    private bool AfterMove()
    {
        PieceColor side = _state.SideToMove;
        switch (MoveGenerator.Status(_state, side))
        {
            case GameStatus.Checkmate:
                _gameOver = true;
                _hud.GameOver($"Checkmate — {Other(side)} wins!");
                break;
            case GameStatus.Stalemate:
                _gameOver = true;
                _hud.GameOver("Stalemate — draw");
                break;
            case GameStatus.Check:
                _hud.SetStatus($"{side} to move — Check!");
                break;
            default:
                _hud.SetStatus($"{side} to move");
                break;
        }
        return !_gameOver;
    }

    private async void MaybeTriggerAi()
    {
        if (_gameOver || !_vsComputer || _state.SideToMove != _aiColor) return;

        await ToSignal(GetTree().CreateTimer(AiThinkTime), SceneTreeTimer.SignalName.Timeout);
        if (_gameOver) return;

        Move? choice = ChessAi.BestMove(_state, _aiColor, AiDepth);
        if (choice is Move move)
        {
            ExecuteMove(move.From, move.To);
            _hud.IncrementMoves();
            AfterMove();
        }
    }

    private static PieceColor Other(PieceColor c) =>
        c == PieceColor.White ? PieceColor.Black : PieceColor.White;

    private void SendToTray(Node3D piece, PieceColor color)
    {
        int idx = color == PieceColor.White ? _capturedWhite++ : _capturedBlack++;
        int slot = idx % 8;
        int lane = idx / 8;
        float z = slot * 0.85f;
        float x = color == PieceColor.White ? 8.5f + lane * 0.9f : -1.5f - lane * 0.9f;
        AnimateTo(piece, new Vector3(x, SurfaceY, z), MoveTime);
    }

    private void AnimateTo(Node3D piece, Vector3 destination, float duration)
    {
        Tween tween = CreateTween();
        tween.TweenProperty(piece, "position", destination, duration)
             .SetTrans(Tween.TransitionType.Sine)
             .SetEase(Tween.EaseType.Out);
    }

    public Node3D SpawnPiece(PieceType type, PieceColor color, int file, int rank)
    {
        string path = $"res://assets/pieces/{type.ToString().ToLower()}.glb";
        Node3D piece = GD.Load<PackedScene>(path).Instantiate<Node3D>();

        piece.Scale = Vector3.One * PieceScale;
        piece.Position = new Vector3(file, SurfaceY, rank);
        piece.Rotation = new Vector3(0f, color == PieceColor.Black ? Mathf.Pi : 0f, 0f);
        piece.Name = $"{color}_{type}_{file}_{rank}";

        Color tint = color == PieceColor.White
            ? new Color(0.92f, 0.90f, 0.85f)
            : new Color(0.18f, 0.16f, 0.16f);
        ApplyTint(piece, tint);

        AddChild(piece);
        _pieces[file, rank] = piece;
        return piece;
    }

    private static void ApplyTint(Node node, Color color)
    {
        if (node is MeshInstance3D mesh)
            mesh.MaterialOverride = new StandardMaterial3D { AlbedoColor = color };
        foreach (Node child in node.GetChildren())
            ApplyTint(child, color);
    }
}
