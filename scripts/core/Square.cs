// A board coordinate: file 0..7 (a..h), rank 0..7 (1..8). Pure data, no Godot.
public readonly record struct Square(int File, int Rank)
{
    public bool IsOnBoard => File is >= 0 and < 8 && Rank is >= 0 and < 8;

    public override string ToString() => $"{(char)('a' + File)}{Rank + 1}";
}
