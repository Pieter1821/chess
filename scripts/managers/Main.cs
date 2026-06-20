using Godot;

public partial class Main : Node3D
{
    public override void _Ready()
    {
        var sun = GetNode<DirectionalLight3D>("Sun");
        sun.RotationDegrees = new Vector3(-50f, -30f, 0f);
        sun.ShadowEnabled = true;

        var rig = GetNode<CameraRig>("CameraRig");
        var pieces = GetNode<PieceSet>("Pieces");
        var menu = GetNode<MainMenu>("MainMenu");

        rig.SetIntroView();   // cinematic overview while the menu is up

        menu.SideChosen += color =>
        {
            rig.SetViewingSide(color);        // swing camera to the player's side
            pieces.StartVsComputer(color);    // computer plays the other side
        };
    }
}
