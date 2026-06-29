# CLAUDE.md

Guidance for AI assistants working in this repository.

## What this is

Conway's Game of Life implemented as a **Windows Forms desktop application** in
C# on **.NET 8** (`net8.0-windows`, `UseWindowsForms`). The simulation logic
lives in a UI-free **.NET 8 class library** (`net8.0`) so it can be unit-tested
on any platform. The app renders by writing pixels directly into a `Bitmap`
shown in a `PictureBox`; each cell maps to a square block of pixels
(`CellSize`). It is a Windows Forms (`WinExe`) app — there is no ASP.NET / web
component.

## Solution layout

Three projects under `TheGameOfLife/`, tied together by `TheGameOfLife.sln`:

```
TheGameOfLife.sln
TheGameOfLife.Core/            # net8.0 — UI-free simulation library
  Cell.cs                  Cell state: Alive + NextAlive
  GameConfig.cs            Rows, Columns, InitialLiveProbability, RandomSeed
  IGameEngine.cs           Engine abstraction (grid, Generation, event, steps)
  GameEngine.cs            Seeding + B3/S23 rules; raises GenerationAdvanced
  GenerationEventArgs.cs   Payload carrying the current generation number
TheGameOfLife/                 # net8.0-windows — WinForms app
  Program.cs               Entry point ([STAThread] Main)
  GameClient.cs            Form: owns the engine, renders, drives the timer
  GameClient.Designer.cs   Designer UI (canvas PictureBox, gameTimer)
  Properties/Resources.*   Generated resources
TheGameOfLife.Tests/           # net8.0 — xUnit tests for the Core library
  GameEngineTests.cs
```

## Platform constraint (important)

The **app** (`TheGameOfLife`) targets `net8.0-windows` with `UseWindowsForms`
and uses `System.Windows.Forms` / `System.Drawing` plus `unsafe` pointer code,
so it **only builds and runs on Windows**.

The **Core library and its tests** target plain `net8.0` and have no Windows
dependency, so `dotnet test` on `TheGameOfLife.Tests` runs anywhere the .NET 8
SDK is available — that is the primary way to verify simulation changes. Note
the sandboxed Linux session used for these tasks has **no .NET SDK installed and
cannot download one** (the Microsoft download host is blocked by egress policy),
so even the Core tests usually can't be executed here. When you change rules,
state the expected test outcome and reason it through; don't claim a build or
test run succeeded if you didn't run it.

## Architecture

The simulation is decoupled from the UI. The engine knows nothing about WinForms
and never calls back into the form through a stored reference — it raises an
event instead:

- `IGameEngine` (in Core) owns the `Cell[,] Cells` grid, `Rows`/`Columns`, a
  `Generation` counter, and the steps `StartGame()` / `NextCycle()`. After each
  it raises `GenerationAdvanced` (an `EventHandler<GenerationEventArgs>`).
- `GameClient` (the form) constructs a `GameEngine`, subscribes to
  `GenerationAdvanced` to repaint, and advances the simulation from a timer.

`Program.Main` calls `ApplicationConfiguration.Initialize()` and runs a
`GameClient`. The form is the only place the two halves meet.

### Lifecycle

1. `GameClient_Load` derives the grid resolution from the window size and
   `CellSize` (`columns = ClientRectangle.Width / CellSize`, likewise rows), then
   creates a 24bpp `Bitmap` sized to an **exact multiple** of `CellSize` so every
   pixel maps to a valid cell.
2. It constructs `new GameEngine(new GameConfig { Rows, Columns })`, subscribes
   to `GenerationAdvanced`, and calls `StartGame()` to seed generation 0.
3. `gameTimer.Tick` is wired to `NextCycle()` and the timer is enabled, so the
   simulation animates on the UI thread's timer pump (no blocking loop).

### Simulation rules (`GameEngine`)

- Seeding: each cell starts alive with probability `InitialLiveProbability`
  (default 0.1) using `Random.NextDouble()`. A non-null `RandomSeed` makes
  seeding **deterministic** — used by tests.
- `GetLivingNeighborCount` sums the 8 Moore neighbours with explicit edge guards;
  off-grid cells count as dead and the grid does **not** wrap.
- `NextCycle` applies B3/S23 in **two parallel passes**: pass 1 computes each
  cell's `NextAlive` from the current grid, pass 2 commits `Alive = NextAlive`.
  Splitting compute from commit keeps neighbour reads consistent during the
  in-place update. (`NextAlive` replaced the old inverted `KillCell` flag.)

### Rendering (`GameClient`)

`UpdateClientBitMapMultiThreadLockbits` uses `unsafe` code with `Bitmap.LockBits`
and `Parallel.For` for fast pixel writes (`AllowUnsafeBlocks` is enabled). Live
cells are painted `DarkRed`, dead cells `LightGray`. Because the bitmap is an
exact multiple of `CellSize`, `pixelsPerCell` equals `CellSize` and the row/column
index derived from a pixel is always in range.

## Conventions

- **Namespaces:** simulation types live in `TheGameOfLife.Core`; the app lives in
  `TheGameOfLife`; tests in `TheGameOfLife.Tests`.
- **Keep the core UI-free:** never add a `System.Windows.Forms` / `System.Drawing`
  dependency to `TheGameOfLife.Core`. UI reacts to the engine via the
  `GenerationAdvanced` event — don't reintroduce a mutual engine↔form reference.
- **Determinism:** anything that needs reproducibility should flow through
  `GameConfig.RandomSeed`; don't introduce new ad-hoc `Random` instances in Core.
- **Style:** PascalCase types/members, 4-space indentation, brace-on-new-line.
  `Nullable` and `ImplicitUsings` are enabled across all three projects.
- **Designer files:** `*.Designer.cs` and `Properties/Resources.Designer.cs` are
  generated. Don't hand-edit them; change UI through the corresponding source.
- **Concurrency:** grid update and rendering both parallelise over rows. If you
  add per-cell mutable state, keep updates split into compute/commit passes so
  parallel reads stay consistent.

## Build, run & test

Requires the .NET 8 SDK (or Visual Studio 2022+):

```
dotnet test  TheGameOfLife/TheGameOfLife.sln                 # Core tests — runs cross-platform
dotnet build TheGameOfLife/TheGameOfLife.sln                 # full build — Windows only (app targets net8.0-windows)
dotnet run   --project TheGameOfLife/TheGameOfLife           # run the app — Windows only
```

On non-Windows hosts the app project won't build (WinForms), but
`dotnet test TheGameOfLife/TheGameOfLife.Tests` builds and runs the Core tests on
their own. There is no CI configuration or linter in the repo; the `.gitignore`
is the standard Visual Studio template (`bin/`, `obj/`, etc.).

## Git workflow for this environment

- Develop on the branch assigned for the session; create it locally if needed.
- Commit with clear messages and push with `git push -u origin <branch>`.
- Do not open a pull request unless explicitly asked.
