using Godot;

// A throwaway script that proves the C# -> Godot pipeline works.
// We'll delete this in Phase 1 once we've confirmed everything runs.
public partial class Hello : Node
{
    public override void _Ready()
    {
        GD.Print("Hello from C#! The .NET pipeline works.");
    }
}
