# Chess Rules — Implementation Checklist

Source of truth: https://en.wikipedia.org/wiki/Rules_of_chess

All of this lives in `scripts/core/` (pure C#, no Godot) and is covered by `tests/`.
The presentation layer (scenes/scripts) only *calls into* and *renders* this logic.

## Board & turn order
- [ ] 8x8 board, files a–h, ranks 1–8; a1 is dark.
- [ ] White moves first; players alternate.
- [ ] Track whose turn it is.

## Piece movement (basic)
- [ ] Pawn: 1 forward (2 from its start), captures diagonally forward.
- [ ] Knight: L-shape (2+1), jumps over pieces.
- [ ] Bishop: diagonals, any distance, blocked by pieces.
- [ ] Rook: ranks/files, any distance, blocked by pieces.
- [ ] Queen: rook + bishop combined.
- [ ] King: one square in any direction.
- [ ] Cannot capture your own pieces; sliding pieces stop at the first blocker.

## Special moves
- [ ] Castling (king + rook): neither has moved, squares between empty, king not in/through/into check.
- [ ] En passant: capture a pawn that just advanced two squares, as if it moved one.
- [ ] Promotion: pawn reaching the last rank becomes Q/R/B/N (default queen).

## Check, checkmate, stalemate
- [ ] Check: the side to move's king is attacked.
- [ ] A move is illegal if it leaves your own king in check.
- [ ] Checkmate: in check with no legal move -> loss.
- [ ] Stalemate: not in check but no legal move -> draw.

## Draw conditions
- [ ] Stalemate.
- [ ] Threefold repetition of position.
- [ ] Fifty-move rule (no capture or pawn move in 50 moves by each side).
- [ ] Insufficient material (e.g. K vs K, K+N vs K, K+B vs K).
- [ ] (Later/optional) mutual agreement.

## Rough phase mapping
- **Phase 5** Selecting pieces (click/highlight) — no rules yet.
- **Phase 6** Moving pieces (free movement + capture) — wire selection to motion.
- **Phase 7** Core engine: legal move generation per piece (basic movement above).
- **Phase 8** Check / checkmate / stalemate.
- **Phase 9** Special moves: castling, en passant, promotion.
- **Phase 10** Draw conditions + game-over states.

## Polish (later, not rules)
- [ ] Auto-flip the camera so each player views from their own side (also tracked in the camera-rig plan).
