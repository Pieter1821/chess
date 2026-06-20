using Godot;

public partial class Hud : CanvasLayer
{
    private Label _status = null!;
    private Label _info = null!;
    private Label _banner = null!;

    private double _elapsed;
    private bool _running;
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

        _banner = new Label { Text = "", Visible = false };
        _banner.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _banner.HorizontalAlignment = HorizontalAlignment.Center;
        _banner.VerticalAlignment = VerticalAlignment.Center;
        _banner.AddThemeFontSizeOverride("font_size", 48);
        _banner.MouseFilter = Control.MouseFilterEnum.Ignore;
        AddChild(_banner);
    }

    public override void _Process(double delta)
    {
        if (!_running) return;
        _elapsed += delta;
        UpdateInfo();
    }

    public void StartClock() => _running = true;

    public void SetStatus(string text) => _status.Text = text;

    public void IncrementMoves()
    {
        _moves++;
        UpdateInfo();
    }

    public void GameOver(string message)
    {
        _running = false;
        _status.Text = "";
        _banner.Text = message;
        _banner.Visible = true;
    }

    private void UpdateInfo()
    {
        int t = (int)_elapsed;
        _info.Text = $"Moves: {_moves}   Time: {t / 60:00}:{t % 60:00}";
    }
}
