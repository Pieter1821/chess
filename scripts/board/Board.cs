using Godot;

public partial class Board : Node3D
{
    public const int Size = 8;            // 8x8 board
    public const float SquareSize = 1.0f; // each square is 1 world unit wide

    private readonly MeshInstance3D[,] _squares = new MeshInstance3D[Size, Size];
    private StandardMaterial3D _light = null!;
    private StandardMaterial3D _dark = null!;
    private StandardMaterial3D _highlight = null!;
    private int _selFile = -1, _selRank = -1;

    public override void _Ready()
    {
        // Shared resources: one box shape, one collision shape, three materials.
        var squareMesh = new BoxMesh { Size = new Vector3(SquareSize, 0.2f, SquareSize) };
        var squareShape = new BoxShape3D { Size = new Vector3(SquareSize, 0.2f, SquareSize) };
        _light = new StandardMaterial3D { AlbedoColor = new Color(0.93f, 0.85f, 0.70f) };
        _dark = new StandardMaterial3D { AlbedoColor = new Color(0.40f, 0.26f, 0.17f) };
        _highlight = new StandardMaterial3D { AlbedoColor = new Color(0.30f, 0.75f, 0.35f) };

        for (int file = 0; file < Size; file++)
        for (int rank = 0; rank < Size; rank++)
        {
            // A physics body so the click-ray can hit it.
            var body = new StaticBody3D
            {
                Name = $"Square_{file}_{rank}",
                Position = new Vector3(file * SquareSize, 0f, rank * SquareSize)
            };
            body.SetMeta("file", file);
            body.SetMeta("rank", rank);

            var mesh = new MeshInstance3D { Mesh = squareMesh, MaterialOverride = BaseMaterial(file, rank) };
            var col = new CollisionShape3D { Shape = squareShape };

            body.AddChild(mesh);
            body.AddChild(col);
            AddChild(body);

            _squares[file, rank] = mesh;
        }
    }

    private StandardMaterial3D BaseMaterial(int file, int rank)
        => (file + rank) % 2 == 1 ? _light : _dark;   // a1 (0,0) is dark

    public void SelectSquare(int file, int rank)
    {
        // Revert the previously selected square to its normal colour.
        if (_selFile >= 0)
            _squares[_selFile, _selRank].MaterialOverride = BaseMaterial(_selFile, _selRank);

        // Highlight the new one.
        _squares[file, rank].MaterialOverride = _highlight;
        _selFile = file;
        _selRank = rank;
    }
}
