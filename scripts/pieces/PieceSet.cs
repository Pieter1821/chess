using System.Collections.Generic;
using Godot;

public partial class PieceSet : Node3D
{
    [Export] public float PieceScale = 18.0f;
    [Export] public float SurfaceY = 0.1f;
    [Export] public float LiftHeight = 0.35f;
    [Export] public float MoveTime = 0.25f;
    [Export] public float LiftTime = 0.12f;
    [Export] public int MoveTimeMs = 300;   // Stockfish think time per move (ms)
    [Export] public int AiDepth = 3;         // difficulty index (1=Easy, 2=Medium, 3=Hard)

    private static readonly PieceType[] BackRank =
    {
        PieceType.Rook, PieceType.Knight, PieceType.Bishop, PieceType.Queen,
        PieceType.King, PieceType.Bishop, PieceType.Knight, PieceType.Rook
    };

    private readonly Node3D?[,] _pieces = new Node3D?[8, 8];
    private BoardState _state = null!;
    private Board _boardView = null!;
    private Hud _hud = null!;
    private MoveHistory _history = null!;
    private StockfishEngine _engine = null!;
    private AudioStreamPlayer _moveSound = null!;

    private Node3D? _selected;
    private Square _selSquare;
    private readonly HashSet<Square> _legalTargets = new();

    private bool _vsComputer;
    private PieceColor _aiColor;
    private PieceColor _playerColor;
    private bool _gameOver;
    private readonly Dictionary<string, int> _positions = new();
    private int _repCount;

    private int _capturedWhite, _capturedBlack;

    public override void _Ready()
    {
        _boardView = GetNode<Board>("../Board");
        _hud = GetNode<Hud>("../Hud");
        _history = GetNode<MoveHistory>("../MoveHistory");
        _engine = GetNode<StockfishEngine>("../Stockfish");
        _hud.DrawRequested += OfferDraw;
        _state = BoardState.CreateStartingPosition();

        _moveSound = new AudioStreamPlayer { Stream = GD.Load<AudioStream>("res://assets/audio/move.mp3") };
        AddChild(_moveSound);

        for (int file = 0; file < 8; file++)
        {
            SpawnPiece(BackRank[file], PieceColor.White, file, 0);
            SpawnPiece(PieceType.Pawn,  PieceColor.White, file, 1);
            SpawnPiece(PieceType.Pawn,  PieceColor.Black, file, 6);
            SpawnPiece(BackRank[file], PieceColor.Black, file, 7);
        }
    }

    public void StartVsComputer(PieceColor playerColor, int depth)
    {
        _vsComputer = true;
        AiDepth = depth;
        _playerColor = playerColor;
        _aiColor = playerColor == PieceColor.White ? PieceColor.Black : PieceColor.White;
        _hud.SetStatus($"{_state.SideToMove} to move");
        UpdateClock();
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
            BeginPlayerMove(_selSquare, sq);
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

    private void BeginPlayerMove(Square from, Square to)
    {
        _selected = null;
        _legalTargets.Clear();
        _boardView.ClearHighlights();

        if (IsPromotion(from, to))
            _hud.ShowPromotion(type => DoMove(from, to, type));   // ask which piece, then move
        else
            DoMove(from, to, null);
    }

    private void DoMove(Square from, Square to, PieceType? promotion)
    {
        ExecuteMove(from, to, promotion);
        _hud.IncrementMoves();
        if (AfterMove()) MaybeTriggerAi();
    }

    private bool IsPromotion(Square from, Square to)
    {
        if (_state[from] is not Piece p || p.Type != PieceType.Pawn) return false;
        return to.Rank == (p.Color == PieceColor.White ? 7 : 0);
    }

    private void ExecuteMove(Square from, Square to, PieceType? promotion = null)
    {
        PlayMoveSound();
        string desc = Notation.ToFriendly(_state, new Move(from, to, promotion));   // board BEFORE the move

        Piece mover = _state[from]!.Value;
        bool isCastle = mover.Type == PieceType.King && Mathf.Abs(to.File - from.File) == 2;
        bool isEnPassant = mover.Type == PieceType.Pawn && from.File != to.File && _pieces[to.File, to.Rank] == null;

        Node3D moving = _pieces[from.File, from.Rank]!;

        // Normal capture on the destination square.
        Node3D? occupant = _pieces[to.File, to.Rank];
        if (occupant != null && _state[to] is Piece captured)
            SendToTray(occupant, captured.Color);

        // En passant: the captured pawn sits beside the destination, on the mover's rank.
        if (isEnPassant)
        {
            var capSq = new Square(to.File, from.Rank);
            Node3D? epPawn = _pieces[capSq.File, capSq.Rank];
            if (epPawn != null && _state[capSq] is Piece epCaptured)
                SendToTray(epPawn, epCaptured.Color);
            _pieces[capSq.File, capSq.Rank] = null;
        }

        _state.ApplyMove(new Move(from, to, promotion));
        _pieces[from.File, from.Rank] = null;

        if (promotion is PieceType && _state[to] is Piece promoted)
        {
            moving.QueueFree();                                          // remove the pawn
            SpawnPiece(promoted.Type, promoted.Color, to.File, to.Rank); // place the promoted piece
        }
        else
        {
            _pieces[to.File, to.Rank] = moving;
            AnimateTo(moving, new Vector3(to.File, SurfaceY, to.Rank), MoveTime);
        }

        // Castling: slide the rook to the far side of the king.
        if (isCastle)
        {
            int rank = from.Rank;
            var rookFrom = to.File == 6 ? new Square(7, rank) : new Square(0, rank);
            var rookTo = to.File == 6 ? new Square(5, rank) : new Square(3, rank);
            Node3D rook = _pieces[rookFrom.File, rookFrom.Rank]!;
            _pieces[rookTo.File, rookTo.Rank] = rook;
            _pieces[rookFrom.File, rookFrom.Rank] = null;
            AnimateTo(rook, new Vector3(rookTo.File, SurfaceY, rookTo.Rank), MoveTime);
        }

        _history.AddMove(desc);
        string posKey = _state.PositionKey();
        _positions.TryGetValue(posKey, out int seen);
        _positions[posKey] = seen + 1;
        _repCount = seen + 1;
    }

    // Updates the HUD for the new side to move; returns false if the game has ended.
    private bool AfterMove()
    {
        PieceColor side = _state.SideToMove;
        GameStatus status = MoveGenerator.Status(_state, side);

        if (status == GameStatus.Checkmate) EndGame($"Checkmate — {Other(side)} wins!");
        else if (status == GameStatus.Stalemate) EndGame("Draw — stalemate");
        else if (MoveGenerator.IsInsufficientMaterial(_state)) EndGame("Draw — insufficient material");
        else if (_state.HalfmoveClock >= 100) EndGame("Draw — 50-move rule");
        else if (_repCount >= 3) EndGame("Draw — threefold repetition");
        else _hud.SetStatus(status == GameStatus.Check ? $"{side} to move — Check!" : $"{side} to move");

        UpdateClock();
        return !_gameOver;
    }

    private void EndGame(string message)
    {
        _gameOver = true;
        _hud.GameOver(message);
    }

    private void OfferDraw()
    {
        if (_gameOver) return;
        EndGame("Draw — agreed");
        UpdateClock();
    }

    // The clock ticks only on the player's turn (and only while the game is live).
    private void UpdateClock() =>
        _hud.SetClockRunning(!_gameOver && _state.SideToMove == _playerColor);

    private void MaybeTriggerAi()
    {
        if (_gameOver || !_vsComputer || _state.SideToMove != _aiColor) return;

        _hud.SetStatus("Computer is thinking...");
        if (_engine.IsReady)
            _engine.RequestBestMove(_state.ToFen(), SkillForDifficulty(AiDepth), MoveTimeMs, OnEngineMove);
        else
            PlayFallbackMove();   // built-in minimax when Stockfish isn't available (e.g. Android)
    }

    private async void PlayFallbackMove()
    {
        await ToSignal(GetTree().CreateTimer(0.3), SceneTreeTimer.SignalName.Timeout);
        if (_gameOver || _state.SideToMove != _aiColor) return;
        Move? choice = ChessAi.BestMove(_state, _aiColor, System.Math.Min(AiDepth + 1, 4));
        if (choice is Move move)
        {
            ExecuteMove(move.From, move.To, move.Promotion);
            AfterMove();
        }
    }

    private void OnEngineMove(string uci)
    {
        if (_gameOver || uci.Length < 4 || _state.SideToMove != _aiColor) return;

        Move move = ParseUci(uci);
        ExecuteMove(move.From, move.To, move.Promotion);
        AfterMove();   // no IncrementMoves — only the player's moves are counted
    }

    private static Move ParseUci(string uci)
    {
        var from = new Square(uci[0] - 'a', uci[1] - '1');
        var to = new Square(uci[2] - 'a', uci[3] - '1');
        PieceType? promotion = uci.Length > 4 ? CharToPiece(uci[4]) : null;
        return new Move(from, to, promotion);
    }

    private static PieceType? CharToPiece(char c) => char.ToLower(c) switch
    {
        'q' => PieceType.Queen,
        'r' => PieceType.Rook,
        'b' => PieceType.Bishop,
        'n' => PieceType.Knight,
        _ => (PieceType?)null,
    };

    private static int SkillForDifficulty(int difficulty) => difficulty switch
    {
        1 => 2,    // Easy
        2 => 8,    // Medium
        _ => 16,   // Hard
    };

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

    private void PlayMoveSound()
    {
        // The source clip is long, so cut it to a short "click" after playing.
        _moveSound.Play();
        SceneTreeTimer timer = GetTree().CreateTimer(0.35);
        timer.Timeout += () => _moveSound.Stop();
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
            : new Color(0.22f, 0.20f, 0.20f);
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
