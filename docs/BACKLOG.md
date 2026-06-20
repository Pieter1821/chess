# Chess — Feature Backlog

Status of the feature list. The game must stay **fun and accessible**, not hard to use.

## Done
- [x] Board + pieces (3D, CC0 assets)
- [x] Pivot camera: orbit / zoom / pan / reset, intro pose, flip to player side
- [x] Click-to-select, legal-move highlighting (green = move, red = capture)
- [x] Move + capture with smooth animation
- [x] Captured pieces shown on the side trays
- [x] Pure-C# rules engine (legal moves) + xUnit tests
- [x] Check / checkmate / stalemate detection
- [x] Winner / draw overlay + "press any key to restart"
- [x] Minimax AI opponent (alpha-beta), difficulty levels
- [x] Start menu: difficulty + choose White/Black
- [x] HUD: player-only move counter + clock, turn/check status
- [x] Move sound (short click)

## 1. Core completeness (next up)
- [ ] **Move history (PGN-style algebraic)** — record every move (e4, Nf3...), scrollable list  <- NEXT
- [ ] Click a move -> jump to that board state
- [ ] Undo / redo (step through full game state)
- [ ] Extra draws: 3-fold repetition, 50-move rule, insufficient material

## 2. Gameplay improvements
- [ ] Last-move highlight (origin + destination)
- [ ] Check highlight on the king
- [ ] Special moves: castling, en passant, promotion (also needed by rules)

## 3. UI / UX upgrades
- [ ] Turn indicator upgrade (color + icon + banner, not just text)
- [ ] Move-history side panel (current move highlighted)
- [ ] Board orientation toggle (manual flip) + coordinates toggle (a-h / 1-8)
- [ ] In-game side menu (pause / new game / options)

## 4. Replay / game state
- [ ] Replay mode (step through a finished game)
- [ ] FEN support (save/load a position)
- [ ] PGN export / import (save & share full games)

## 5. Polish
- [ ] Distinct sounds: move / capture / check / checkmate
- [ ] Smooth capture animation (fade/slide)
- [ ] Save / load local games (files in `user://`)

## 6. Online (own phase, later)
- [ ] Login / register + cloud saves + leaderboards
- Recommendation: **Supabase** (Postgres + Auth + REST + row-level security), called from Godot via `HTTPRequest`. Local-first features come first; this is a dedicated phase.

## AI / engine notes
- Current AI: in-house minimax (depth = difficulty). Stockfish-via-UCI remains the upgrade path for a strong opponent.
- Keep "Easy" genuinely easy for casual players.
