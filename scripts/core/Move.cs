// A move is a from-square to a to-square. (Promotion/castle flags come later.)
public readonly record struct Move(Square From, Square To, PieceType? Promotion = null);
