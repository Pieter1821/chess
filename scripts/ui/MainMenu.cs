using System;
using Godot;

public partial class MainMenu : CanvasLayer
{
    // Announced when the player picks a side; Main wires this to the camera.
    public event Action<PieceColor>? SideChosen;

    public override void _Ready()
    {
        // Dim full-screen backdrop that also blocks clicks to the board behind it.
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

        var box = new VBoxContainer { CustomMinimumSize = new Vector2(260, 0) };
        box.AddThemeConstantOverride("separation", 16);
        center.AddChild(box);

        var title = new Label { Text = "C H E S S", HorizontalAlignment = HorizontalAlignment.Center };
        title.AddThemeFontSizeOverride("font_size", 48);
        box.AddChild(title);

        var prompt = new Label { Text = "Choose your side", HorizontalAlignment = HorizontalAlignment.Center };
        box.AddChild(prompt);

        box.AddChild(MakeButton("Play as White", PieceColor.White));
        box.AddChild(MakeButton("Play as Black", PieceColor.Black));
    }

    private Button MakeButton(string text, PieceColor color)
    {
        var button = new Button { Text = text, CustomMinimumSize = new Vector2(0, 52) };
        button.Pressed += () => Choose(color);
        return button;
    }

    private void Choose(PieceColor color)
    {
        SideChosen?.Invoke(color);
        QueueFree();   // dismiss the menu
    }
}
