using Godot;

public partial class PieceSet : Node3D
{
    [Export] public float PieceScale = 18.0f;
    [Export] public float SurfaceY = 0.1f;     // top surface of the board squares
    [Export] public float LiftHeight = 0.35f;  // how far a selected piece rises
    [Export] public float MoveTime = 0.25f;    // seconds for a move glide
    [Export] public float LiftTime = 0.12f;    // seconds for select/deselect

    private static readonly PieceType[] BackRank =
    {
        PieceType.Rook, PieceType.Knight, PieceType.Bishop, PieceType.Queen,
        PieceType.King, PieceType.Bishop, PieceType.Knight, PieceType.Rook
    };

    private readonly Node3D?[,] _pieces = new Node3D?[8, 8];
    private Node3D? _selected;
    private int _selFile = -1, _selRank = -1;

    public override void _Ready()
    {
        for (int file = 0; file < 8; file++)
        {
            SpawnPiece(BackRank[file], PieceColor.White, file, 0);
            SpawnPiece(PieceType.Pawn,  PieceColor.White, file, 1);
            SpawnPiece(PieceType.Pawn,  PieceColor.Black, file, 6);
            SpawnPiece(BackRank[file], PieceColor.Black, file, 7);
        }
    }

    public Node3D? GetPieceAt(int file, int rank) => _pieces[file, rank];

    public void HandleClick(int file, int rank)
    {
        if (_selected == null)
        {
            // Nothing selected: pick up a piece if one is here.
            Node3D? piece = _pieces[file, rank];
            if (piece != null) Select(piece, file, rank);
            return;
        }

        if (file == _selFile && rank == _selRank)
        {
            Deselect();          // clicked the same piece -> put it back down (cancel)
            return;
        }

        MoveSelectedTo(file, rank);   // move (capturing any occupant)
    }

    private void Select(Node3D piece, int file, int rank)
    {
        _selected = piece;
        _selFile = file;
        _selRank = rank;
        AnimateTo(piece, new Vector3(file, SurfaceY + LiftHeight, rank), LiftTime);
    }

    private void Deselect()
    {
        if (_selected != null)
        {
            AnimateTo(_selected, new Vector3(_selFile, SurfaceY, _selRank), LiftTime);
            _selected = null;
            _selFile = _selRank = -1;
        }
    }

    private void MoveSelectedTo(int destFile, int destRank)
    {
        Node3D moving = _selected!;

        // Capture whatever stands on the destination.
        Node3D? occupant = _pieces[destFile, destRank];
        if (occupant != null && occupant != moving)
            occupant.QueueFree();

        // Update the grid (source of truth): old square empties, new square holds this piece.
        _pieces[_selFile, _selRank] = null;
        _pieces[destFile, destRank] = moving;

        // Glide it down onto the destination square.
        AnimateTo(moving, new Vector3(destFile, SurfaceY, destRank), MoveTime);

        _selected = null;
        _selFile = _selRank = -1;
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
