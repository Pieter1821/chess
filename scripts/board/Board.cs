using Godot;

public partial class Board : Node3D
{
    private const int Size = 8;            // 8x8 board
    private const float SquareSize = 1.0f; // each square is 1 world unit wide

    public override void _Ready()
    {
        // One shared shape for every square (a thin box).
        var squareMesh = new BoxMesh
        {
            Size = new Vector3(SquareSize, 0.2f, SquareSize)
        };

        // Two shared surfaces. Materials are Resources = reusable data.
        var lightMaterial = new StandardMaterial3D { AlbedoColor = new Color(0.93f, 0.85f, 0.70f) };
        var darkMaterial = new StandardMaterial3D { AlbedoColor = new Color(0.40f, 0.26f, 0.17f) };

        for (int file = 0; file < Size; file++)        // columns a..h  -> x
        {
            for (int rank = 0; rank < Size; rank++)    // rows 1..8     -> z
            {
                var square = new MeshInstance3D
                {
                    Mesh = squareMesh,
                    Position = new Vector3(file * SquareSize, 0f, rank * SquareSize),
                    Name = $"Square_{file}_{rank}"
                };

                bool isLight = (file + rank) % 2 == 1; // checkerboard; a1 is dark
                square.MaterialOverride = isLight ? lightMaterial : darkMaterial;

                AddChild(square);                      // attach to the tree -> it appears
            }
        }
    }
}
