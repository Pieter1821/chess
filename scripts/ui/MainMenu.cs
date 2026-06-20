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
        var bg = new ColorRect
        {
            Color = new Color(0.05f, 0.05f, 0.07f, 0.6f),
            MouseFilter = Control.MouseFilterEnum.Stop
        };
        bg.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(bg);

        var center = new CenterContainer();
        center.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(center);

        var box = new VBoxContainer { CustomMinimumSize = new Vector2(300, 0) };
        box.AddThemeConstantOverride("separation", 14);
        center.AddChild(box);

        var title = new Label { Text = "C H E S S", HorizontalAlignment = HorizontalAlignment.Center };
        title.AddThemeFontSizeOverride("font_size", 48);
        box.AddChild(title);

        _diffLabel = new Label { Text = "Difficulty: Medium", HorizontalAlignment = HorizontalAlignment.Center };
        box.AddChild(_diffLabel);

        var diffRow = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        diffRow.AddThemeConstantOverride("separation", 8);
        diffRow.AddChild(DifficultyButton("Easy", 1));
        diffRow.AddChild(DifficultyButton("Medium", 2));
        diffRow.AddChild(DifficultyButton("Hard", 3));
        box.AddChild(diffRow);

        var prompt = new Label { Text = "Choose your side", HorizontalAlignment = HorizontalAlignment.Center };
        box.AddChild(prompt);

        box.AddChild(SideButton("Play as White", PieceColor.White));
        box.AddChild(SideButton("Play as Black", PieceColor.Black));
    }

    private Button DifficultyButton(string label, int depth)
    {
        var button = new Button { Text = label, CustomMinimumSize = new Vector2(86, 40) };
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
