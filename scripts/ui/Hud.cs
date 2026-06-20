using System;
using Godot;

public partial class Hud : CanvasLayer
{
    private Label _status = null!;
    private Label _info = null!;
    private ColorRect _overlay = null!;
    private CenterContainer _endScreen = null!;
    private Label _banner = null!;
    private Button _drawButton = null!;

    public event Action? DrawRequested;

    private ColorRect _promoOverlay = null!;
    private CenterContainer _promoScreen = null!;
    private Action<PieceType>? _promoCallback;

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

        _drawButton = new Button
        {
            Text = "Offer Draw",
            AnchorTop = 1f,
            AnchorBottom = 1f,
            OffsetLeft = 16f,
            OffsetRight = 132f,
            OffsetTop = -52f,
            OffsetBottom = -16f,
        };
        _drawButton.Pressed += () => DrawRequested?.Invoke();
        AddChild(_drawButton);

        _promoOverlay = new ColorRect { Color = new Color(0f, 0f, 0f, 0.55f), Visible = false };
        _promoOverlay.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _promoOverlay.MouseFilter = Control.MouseFilterEnum.Stop;
        AddChild(_promoOverlay);

        _promoScreen = new CenterContainer { Visible = false };
        _promoScreen.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(_promoScreen);

        var pbox = new VBoxContainer();
        pbox.AddThemeConstantOverride("separation", 12);
        _promoScreen.AddChild(pbox);

        var plabel = new Label { Text = "Promote pawn to:", HorizontalAlignment = HorizontalAlignment.Center };
        plabel.AddThemeFontSizeOverride("font_size", 28);
        pbox.AddChild(plabel);

        var prow = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        prow.AddThemeConstantOverride("separation", 8);
        pbox.AddChild(prow);
        prow.AddChild(PromoButton("Queen", PieceType.Queen));
        prow.AddChild(PromoButton("Rook", PieceType.Rook));
        prow.AddChild(PromoButton("Bishop", PieceType.Bishop));
        prow.AddChild(PromoButton("Knight", PieceType.Knight));
    }

    public void ShowPromotion(Action<PieceType> callback)
    {
        _promoCallback = callback;
        _promoOverlay.Visible = true;
        _promoScreen.Visible = true;
    }

    private Button PromoButton(string text, PieceType type)
    {
        var button = new Button { Text = text, CustomMinimumSize = new Vector2(96, 48) };
        button.Pressed += () => ChoosePromotion(type);
        return button;
    }

    private void ChoosePromotion(PieceType type)
    {
        _promoOverlay.Visible = false;
        _promoScreen.Visible = false;
        Action<PieceType>? cb = _promoCallback;
        _promoCallback = null;
        cb?.Invoke(type);
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
        _drawButton.Visible = false;
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
