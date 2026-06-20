using Godot;

public partial class Hud : CanvasLayer
{
    private Label _status = null!;
    private Label _info = null!;
    private ColorRect _overlay = null!;
    private CenterContainer _endScreen = null!;
    private Label _banner = null!;

    private double _elapsed;
    private bool _running;
    private bool _gameOver;
    private int _moves;

    public override void _Ready()
    {
        _info = new Label { Text = "Moves: 0   Time: 00:00", Position = new Vector2(16, 12) };
        AddChild(_info);

        _status = new Label { Text = "" };
        _status.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _status.HorizontalAlignment = HorizontalAlignment.Center;
        _status.VerticalAlignment = VerticalAlignment.Top;
        _status.MouseFilter = Control.MouseFilterEnum.Ignore;
        AddChild(_status);

        _overlay = new ColorRect { Color = new Color(0f, 0f, 0f, 0.55f), Visible = false };
        _overlay.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _overlay.MouseFilter = Control.MouseFilterEnum.Ignore;
        AddChild(_overlay);

        _endScreen = new CenterContainer { Visible = false };
        _endScreen.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _endScreen.MouseFilter = Control.MouseFilterEnum.Ignore;
        AddChild(_endScreen);

        var box = new VBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        box.AddThemeConstantOverride("separation", 12);
        _endScreen.AddChild(box);

        _banner = new Label { Text = "", HorizontalAlignment = HorizontalAlignment.Center };
        _banner.AddThemeFontSizeOverride("font_size", 52);
        box.AddChild(_banner);

        var subtitle = new Label { Text = "Press any key to play again", HorizontalAlignment = HorizontalAlignment.Center };
        subtitle.AddThemeFontSizeOverride("font_size", 20);
        box.AddChild(subtitle);
    }

    public override void _Process(double delta)
    {
        if (!_running) return;
        _elapsed += delta;
        UpdateInfo();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!_gameOver) return;
        bool pressed = (@event is InputEventKey k && k.Pressed)
                    || (@event is InputEventMouseButton mb && mb.Pressed);
        if (pressed) GetTree().ReloadCurrentScene();   // restart the whole game
    }

    public void SetClockRunning(bool running) => _running = running;

    public void SetStatus(string text) => _status.Text = text;

    public void IncrementMoves()
    {
        _moves++;
        UpdateInfo();
    }

    public void GameOver(string message)
    {
        _running = false;
        _gameOver = true;
        _status.Text = "";
        _banner.Text = message;
        _overlay.Visible = true;
        _endScreen.Visible = true;
    }

    private void UpdateInfo()
    {
        int t = (int)_elapsed;
        _info.Text = $"Moves: {_moves}   Time: {t / 60:00}:{t % 60:00}";
    }
}
