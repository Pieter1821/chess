using System;
using Godot;

public partial class MainMenu : CanvasLayer
{
    // Announced when the player starts a game: (side, ai search depth).
    public event Action<PieceColor, int>? GameStarted;

    private int _depth = 2;   // Medium by default (kept beatable for casual play)
    private Label _diffLabel = null!;

    public override void _Ready()
    {
        // Splash artwork as the menu background.
        var bg = new TextureRect
        {
            Texture = GD.Load<Texture2D>("res://assets/art/splash.png"),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered,
            MouseFilter = Control.MouseFilterEnum.Stop,
        };
        bg.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(bg);

        // A subtle dark scrim so the buttons stay readable over the art.
        var scrim = new ColorRect { Color = new Color(0f, 0f, 0f, 0.4f), MouseFilter = Control.MouseFilterEnum.Ignore };
        scrim.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(scrim);

        var center = new CenterContainer();
        center.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(center);

        var panel = new PanelContainer();
        center.AddChild(panel);

        var box = new VBoxContainer { CustomMinimumSize = new Vector2(320, 0) };
        box.AddThemeConstantOverride("separation", 14);
        panel.AddChild(box);

        _diffLabel = new Label { Text = "Difficulty: Medium", HorizontalAlignment = HorizontalAlignment.Center };
        _diffLabel.AddThemeFontSizeOverride("font_size", 22);
        box.AddChild(_diffLabel);

        var diffRow = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        diffRow.AddThemeConstantOverride("separation", 8);
        diffRow.AddChild(DifficultyButton("Easy", 1));
        diffRow.AddChild(DifficultyButton("Medium", 2));
        diffRow.AddChild(DifficultyButton("Hard", 3));
        box.AddChild(diffRow);

        var prompt = new Label { Text = "Choose your side", HorizontalAlignment = HorizontalAlignment.Center };
        prompt.AddThemeFontSizeOverride("font_size", 22);
        box.AddChild(prompt);

        box.AddChild(SideButton("Play as White", PieceColor.White));
        box.AddChild(SideButton("Play as Black", PieceColor.Black));
    }

    private Button DifficultyButton(string label, int depth)
    {
        var button = new Button { Text = label, CustomMinimumSize = new Vector2(90, 40) };
        button.Pressed += () =>
        {
            _depth = depth;
            _diffLabel.Text = $"Difficulty: {label}";
        };
        return button;
    }

    private Button SideButton(string text, PieceColor color)
    {
        var button = new Button { Text = text, CustomMinimumSize = new Vector2(0, 50) };
        button.Pressed += () =>
        {
            GameStarted?.Invoke(color, _depth);
            QueueFree();
        };
        return button;
    }
}
