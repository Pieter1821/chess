using Godot;

public partial class Main : Node3D
{
    public override void _Ready()
    {
        var sun = GetNode<DirectionalLight3D>("Sun");
        sun.RotationDegrees = new Vector3(-50f, -30f, 0f);
        sun.ShadowEnabled = true;
    }
}
