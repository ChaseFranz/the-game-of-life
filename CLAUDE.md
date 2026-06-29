# CLAUDE.md

Guidance for AI assistants working in this repository.

## What this is

Conway's Game of Life implemented as a **Windows Forms desktop application** in
C# on **.NET Core 3.1** (`netcoreapp3.1`, `UseWindowsForms`). The simulation is
rendered by drawing directly into a `Bitmap` and showing it in a `PictureBox`,
so each grid cell maps to a block of pixels on a maximized window.

> Note: `README.md` describes the project as "An ASP.Net Core implementation."
> That is inaccurate — there is no ASP.NET / web component. It is a Windows
> Forms (`WinExe`) app. Keep this in mind; treat the code as the source of truth.

## Platform constraint (important)

This project targets `Microsoft.NET.Sdk.WindowsDesktop` with `UseWindowsForms`
and uses `System.Windows.Forms` / `System.Drawing` plus `unsafe` pointer code.
**It only builds and runs on Windows.** It cannot be built or run on the Linux
environment these sessions typically run in. When asked to "run" or "test"
changes, you generally cannot execute the app here — reason about correctness by
reading the code and, where possible, verify syntax/logic statically. Make this
limitation clear rather than pretending a build succeeded.

## Layout

```
TheGameOfLife.sln                         Solution (single project)
TheGameOfLife/TheGameOfLife/
  Program.cs            App entry point ([STAThread] Main); wires GameClient + GameEngine
  IClient.cs           UI abstraction: GameEngine ref + RefreshClient()
  IGameEngine.cs       Engine abstraction: grid, dimensions, StartGame()/NextCycle()
  GameEngine.cs        Simulation: cell generation + Game of Life rules
  Cell.cs              Single cell: Alive + KillCell flags
  GameClient.cs        WinForms Form; renders the grid into a bitmap
  GameClient.Designer.cs   Designer-generated UI (canvas PictureBox, gameTimer)
  Properties/Resources.*   Generated resources
```

## Architecture

The UI and the simulation are decoupled through two interfaces that hold mutual
references to each other:

- `IGameEngine` owns the `Cell[,] Cells` grid, `GridHeight`/`GridWidth`, and the
  simulation steps `StartGame()` / `NextCycle()`. It calls back into the client
  via `GameClient.RefreshClient()` after each step.
- `IClient` (implemented by the `GameClient` form) owns a reference to the
  engine and exposes `RefreshClient()` to repaint.

`Program.Main` constructs `new GameClient(new GameEngine())`; the `GameClient`
constructor sets `GameEngine.GameClient = this`, completing the two-way wiring.

### Lifecycle

1. `GameClient_Load` sizes the canvas to the (maximized) client rectangle and
   creates a 24bpp `Bitmap`. Grid dimensions are set equal to the bitmap's pixel
   dimensions (`GridHeight = Image.Height`, `GridWidth = Image.Width`).
2. `GameEngine.StartGame()` allocates `Cells` and calls `RefreshClient()`.
3. The load handler then runs a fixed loop of **1000** `NextCycle()` iterations.
   (`gameTimer` is enabled but currently has no `Tick` handler wired up, so the
   1000-iteration loop is what actually advances the simulation.)

### Simulation rules (`GameEngine`)

- `Cell.Alive` is seeded randomly with ~1/10 probability (`randomObj.Next(0,10) == 0`).
- `GetLivingNeighborCount` sums the 8 Moore neighbors with explicit edge guards
  (cells off-grid count as dead; the grid does **not** wrap).
- `UpdateGridState` applies the standard B3/S23 rule in **two passes** to avoid
  in-place corruption: pass 1 computes each cell's `KillCell` flag, pass 2 sets
  `Alive = !KillCell`. Both passes use `Parallel.For` over rows.

### Rendering (`GameClient`)

`UpdateClientBitMapMultiThreadLockbits` uses `unsafe` code with `Bitmap.LockBits`
and `Parallel.For` for fast pixel writes (`AllowUnsafeBlocks` is enabled in the
csproj). Live cells are painted `DarkRed`, dead cells `LightGray`. Because grid
dimensions equal the bitmap dimensions, `pixelsPerCell` is effectively 1.

## Conventions

- **Namespace:** everything is in `TheGameOfLife`.
- **Style:** standard C# / Visual Studio conventions — PascalCase types and
  members, 4-space indentation, brace-on-new-line, auto-properties.
- **Designer files:** `*.Designer.cs` and `Properties/Resources.Designer.cs` are
  generated. Don't hand-edit them; change UI through the corresponding source.
- **Interfaces first:** keep the `IClient` / `IGameEngine` seam intact when
  changing behavior — new engine/UI capabilities should go through the interfaces.
- **Concurrency:** the grid update and rendering both parallelize over rows. If
  you add per-cell mutable state, keep updates split into compute/apply passes so
  parallel reads stay consistent.

## Build & run

Windows + .NET Core 3.1 SDK (or Visual Studio 2019+) required:

```
dotnet build TheGameOfLife/TheGameOfLife.sln       # or open the .sln in Visual Studio
dotnet run --project TheGameOfLife/TheGameOfLife    # Windows only
```

There is **no test project, CI configuration, or linter** in the repo. The
`.gitignore` is the standard Visual Studio template (`bin/`, `obj/`, etc.).

## Git workflow for this environment

- Develop on the branch assigned for the session; create it locally if needed.
- Commit with clear messages and push with `git push -u origin <branch>`.
- Do not open a pull request unless explicitly asked.
