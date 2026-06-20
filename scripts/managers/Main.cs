using Godot;

public partial class Main : Node3D
{
    public override void _Ready()
    {
        var sun = GetNode<DirectionalLight3D>("Sun");
        sun.RotationDegrees = new Vector3(-50f, -30f, 0f);
        sun.ShadowEnabled = true;
        sun.LightEnergy = 1.2f;

        // Ambient fill + background so pieces don't sink into shadow.
        var env = new Godot.Environment
        {
            BackgroundMode = Godot.Environment.BGMode.Color,
            BackgroundColor = new Color(0.20f, 0.22f, 0.27f),
            AmbientLightSource = Godot.Environment.AmbientSource.Color,
            AmbientLightColor = new Color(0.72f, 0.74f, 0.80f),
            AmbientLightEnergy = 0.6f,
        };
        AddChild(new WorldEnvironment { Environment = env });

        // A soft fill light from the opposite side, no shadows, to lift the dark faces.
        var fill = new DirectionalLight3D
        {
            RotationDegrees = new Vector3(-25f, 140f, 0f),
            LightEnergy = 0.35f,
            ShadowEnabled = false,
        };
        AddChild(fill);

        var rig = GetNode<CameraRig>("CameraRig");
        var pieces = GetNode<PieceSet>("Pieces");
        var menu = GetNode<MainMenu>("MainMenu");

        rig.SetIntroView();   // cinematic overview while the menu is up

        menu.GameStarted += (color, depth) =>
        {
            rig.SetViewingSide(color);              // swing camera to the player's side
            pieces.StartVsComputer(color, depth);   // computer plays the other side
        };
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is not InputEventKey key || !key.Pressed) return;

        if (key.Keycode == Key.F11)
        {
            ToggleFullscreen();
        }
        else if (key.Keycode == Key.Escape
                 && DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen)
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
        }
    }

    private static void ToggleFullscreen()
    {
        bool isFullscreen = DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen;
        DisplayServer.WindowSetMode(isFullscreen
            ? DisplayServer.WindowMode.Windowed
            : DisplayServer.WindowMode.Fullscreen);
    }
}
