using Godot;

public partial class Main : Node3D
{
    public override void _Ready()
    {
        var camera = GetNode<Camera3D>("Camera3D");
        camera.Position = new Vector3(3.5f, 10f, 13f);          // up and behind the board
        camera.LookAt(new Vector3(3.5f, 0f, 3.5f), Vector3.Up); // aim at the board's center

        var sun = GetNode<DirectionalLight3D>("Sun");
        sun.RotationDegrees = new Vector3(-50f, -30f, 0f);      // angle the light down
        sun.ShadowEnabled = true;
    }
}
