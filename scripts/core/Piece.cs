// A chess piece is simply a type + a color. Pure data, no Godot.
public readonly record struct Piece(PieceType Type, PieceColor Color);
