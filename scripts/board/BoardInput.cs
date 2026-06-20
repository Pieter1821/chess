using Godot;

public partial class BoardInput : Node
{
    private Board _board = null!;
    private PieceSet _pieces = null!;

    public override void _Ready()
    {
        _board = GetNode<Board>("../Board");
        _pieces = GetNode<PieceSet>("../Pieces");
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left && mb.Pressed)
        {
            Camera3D cam = GetViewport().GetCamera3D();
            if (cam == null) return;

            // Build a ray from the camera through the mouse pixel.
            Vector3 from = cam.ProjectRayOrigin(mb.Position);
            Vector3 to = from + cam.ProjectRayNormal(mb.Position) * 100f;

            // Ask the physics world what it hits first.
            var query = PhysicsRayQueryParameters3D.Create(from, to);
            Godot.Collections.Dictionary hit = cam.GetWorld3D().DirectSpaceState.IntersectRay(query);

            if (hit.Count > 0)
            {
                Node collider = hit["collider"].As<Node>();
                if (collider != null && collider.HasMeta("file"))
                {
                    int file = collider.GetMeta("file").AsInt32();
                    int rank = collider.GetMeta("rank").AsInt32();
                    _board.SelectSquare(file, rank);     // highlight the tile
                    _pieces.HandleClick(file, rank);     // select/lift the piece on it
                }
            }
        }
    }
}
