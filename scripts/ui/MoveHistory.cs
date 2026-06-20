using System.Collections.Generic;
using System.Text;
using Godot;

public partial class MoveHistory : CanvasLayer
{
    private RichTextLabel _list = null!;
    private readonly List<string> _moves = new();

    public override void _Ready()
    {
        var panel = new PanelContainer
        {
            AnchorLeft = 1f,
            AnchorRight = 1f,
            AnchorBottom = 1f,
            OffsetLeft = -240f,
            OffsetRight = -8f,
            OffsetTop = 48f,
            OffsetBottom = -8f,
        };
        AddChild(panel);

        var vbox = new VBoxContainer();
        panel.AddChild(vbox);

        var title = new Label { Text = "Moves" };
        title.AddThemeFontSizeOverride("font_size", 20);
        vbox.AddChild(title);

        _list = new RichTextLabel
        {
            ScrollActive = true,
            ScrollFollowing = true,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
        };
        _list.AddThemeFontSizeOverride("normal_font_size", 16);
        vbox.AddChild(_list);
    }

    public void AddMove(string san)
    {
        _moves.Add(san);
        _list.Text = BuildText();
    }

    private string BuildText()
    {
        var sb = new StringBuilder();
        for (int i = 0; i < _moves.Count; i++)
        {
            // White plies get the move number; Black replies are indented under them.
            string prefix = (i % 2 == 0) ? $"{i / 2 + 1}.  " : "     ";
            sb.Append($"{prefix}{_moves[i]}\n");
        }
        return sb.ToString();
    }
}
