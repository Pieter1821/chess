using Godot;

public partial class BoardInput : Node
{
    [Export] public float BoardSurfaceY = 0.1f;   // height of the squares' top surface

    private PieceSet _pieces = null!;

    public override void _Ready() => _pieces = GetNode<PieceSet>("../Pieces");

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is not InputEventMouseButton mb || mb.ButtonIndex != MouseButton.Left || !mb.Pressed)
            return;

        Camera3D cam = GetViewport().GetCamera3D();
        if (cam == null) return;

        // Intersect the click ray with the board plane and read off the square.
        // (More reliable than hitting thin tile colliders past tall pieces.)
        Vector3 origin = cam.ProjectRayOrigin(mb.Position);
        Vector3 dir = cam.ProjectRayNormal(mb.Position);
        if (Mathf.IsZeroApprox(dir.Y)) return;

        float t = (BoardSurfaceY - origin.Y) / dir.Y;
        if (t < 0f) return;                       // plane is behind the camera
        Vector3 point = origin + dir * t;

        int file = Mathf.RoundToInt(point.X);
        int rank = Mathf.RoundToInt(point.Z);
        if (file is < 0 or > 7 || rank is < 0 or > 7) return;   // clicked off the board (e.g. a tray)

        _pieces.HandleClick(file, rank);
    }
}
