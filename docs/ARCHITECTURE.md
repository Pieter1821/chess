# Chess — Architecture & Conventions

A 3D chess game built in Godot 4.6.3 (.NET) with C#.

## Guiding principle: separate logic from presentation

- **`scripts/core/`** holds the *pure* rules of chess as plain C# classes with **no
  Godot dependency** (no `using Godot;` for game-state types). It models the board,
  pieces, moves, and legality. It is fast to compile and unit-testable in isolation.
- **Everything else** (scenes + node scripts) is the *presentation* layer: it draws the
  state from `core`, and translates player input into calls on `core`.

If a class needs to know a rule of chess, it belongs in `core`. If it needs to know
about a Node, camera, mesh, or click, it belongs in the presentation layer.

## Folder layout

| Folder | Holds | Notes |
|--------|-------|-------|
| `assets/` | Imported art, audio, fonts | Sourced from the Godot Asset Library |
| `scenes/` | `.tscn` scene files | "Things in the world" — board, pieces, UI |
| `scripts/core/` | Pure chess logic | No Godot dependency; covered by `tests/` |
| `scripts/board/`, `pieces/`, `ui/`, `managers/` | Node behavior | Bridges `core` to the scene tree |
| `resources/` | `.tres` data | Themes, piece definitions, settings |
| `docs/` | Design & architecture notes | This file |
| `tests/` | Unit tests for `scripts/core` | |

## Naming conventions

- **Folders & scenes:** `snake_case` (e.g. `scenes/pieces/white_pawn.tscn`).
  Lowercase avoids case-sensitivity bugs on Linux/macOS export targets.
- **C# files & classes:** `PascalCase`, file name matches class name (`Board.cs` -> `class Board`).
- **C# namespaces** mirror folders under `scripts/`: `Chess.Core`, `Chess.Pieces`, etc.

## Key Godot concepts used here

- **Scene (`.tscn`)** = a tree of Nodes; also serves as Godot's "prefab".
- **Script (`.cs`)** = behavior attached to a node.
- **Resource (`.tres`)** = pure data, no behavior.
