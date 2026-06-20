using Godot;

public partial class BoardInput : Node
{
    private PieceSet _pieces = null!;

    public override void _Ready() => _pieces = GetNode<PieceSet>("../Pieces");

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left && mb.Pressed)
        {
            Camera3D cam = GetViewport().GetCamera3D();
            if (cam == null) return;

            Vector3 from = cam.ProjectRayOrigin(mb.Position);
            Vector3 to = from + cam.ProjectRayNormal(mb.Position) * 100f;

            var query = PhysicsRayQueryParameters3D.Create(from, to);
            Godot.Collections.Dictionary hit = cam.GetWorld3D().DirectSpaceState.IntersectRay(query);

            if (hit.Count > 0)
            {
                Node collider = hit["collider"].As<Node>();
                if (collider != null && collider.HasMeta("file"))
                {
                    int file = collider.GetMeta("file").AsInt32();
                    int rank = collider.GetMeta("rank").AsInt32();
                    _pieces.HandleClick(file, rank);
                }
            }
        }
    }
}
