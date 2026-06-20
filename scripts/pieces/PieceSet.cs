using Godot;

public partial class PieceSet : Node3D
{
    [Export] public float PieceScale = 18.0f;
    [Export] public float SurfaceY = 0.1f;    // top surface of the board squares
    [Export] public float LiftHeight = 0.35f; // how far a selected piece rises

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
        Node3D? piece = _pieces[file, rank];
        if (piece != null && piece != _selected)
        {
            Deselect();
            _selected = piece;
            _selFile = file;
            _selRank = rank;
            piece.Position = new Vector3(file, SurfaceY + LiftHeight, rank);   // lift it
        }
        else
        {
            Deselect();   // clicked empty square or the already-selected piece
        }
    }

    private void Deselect()
    {
        if (_selected != null)
        {
            _selected.Position = new Vector3(_selFile, SurfaceY, _selRank);    // set it back down
            _selected = null;
            _selFile = _selRank = -1;
        }
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
