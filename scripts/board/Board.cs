using Godot;
using System.Collections.Generic;

public partial class Board : Node3D
{
    public const int Size = 8;
    public const float SquareSize = 1.0f;

    public enum HighlightKind { Selected, Move, Capture }

    private readonly MeshInstance3D[,] _squares = new MeshInstance3D[Size, Size];
    private readonly List<(int f, int r)> _highlighted = new();

    private StandardMaterial3D _light = null!, _dark = null!;
    private StandardMaterial3D _selected = null!, _move = null!, _capture = null!;

    public override void _Ready()
    {
        var squareMesh = new BoxMesh { Size = new Vector3(SquareSize, 0.2f, SquareSize) };
        var squareShape = new BoxShape3D { Size = new Vector3(SquareSize, 0.2f, SquareSize) };
        _light = new StandardMaterial3D { AlbedoColor = new Color(0.93f, 0.85f, 0.70f) };
        _dark = new StandardMaterial3D { AlbedoColor = new Color(0.40f, 0.26f, 0.17f) };
        _selected = new StandardMaterial3D { AlbedoColor = new Color(0.95f, 0.85f, 0.30f) }; // yellow
        _move = new StandardMaterial3D { AlbedoColor = new Color(0.30f, 0.75f, 0.35f) };     // green
        _capture = new StandardMaterial3D { AlbedoColor = new Color(0.85f, 0.30f, 0.25f) };  // red

        for (int file = 0; file < Size; file++)
        for (int rank = 0; rank < Size; rank++)
        {
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
        => (file + rank) % 2 == 1 ? _light : _dark;

    public void ClearHighlights()
    {
        foreach (var (f, r) in _highlighted)
            _squares[f, r].MaterialOverride = BaseMaterial(f, r);
        _highlighted.Clear();
    }

    public void SetHighlight(int file, int rank, HighlightKind kind)
    {
        _squares[file, rank].MaterialOverride = kind switch
        {
            HighlightKind.Selected => _selected,
            HighlightKind.Capture => _capture,
            _ => _move,
        };
        _highlighted.Add((file, rank));
    }
}
