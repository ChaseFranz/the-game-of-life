# the-game-of-life
A Windows Forms desktop implementation of Conway's Game of Life, built in C# on .NET 8.

The simulation logic lives in a UI-free class library (`TheGameOfLife.Core`) and
is exercised by a WinForms front end (`TheGameOfLife`). The front end renders by
writing pixels directly into a `Bitmap` (via `unsafe` `LockBits` and
`Parallel.For`) and displaying it in a `PictureBox`. Each cell is drawn as a
square block of pixels, and the grid advances on a timer. The engine applies the
standard B3/S23 rules in two parallel passes and notifies the UI through an
event, so the core has no dependency on Windows Forms.

## Projects

- `TheGameOfLife.Core` (`net8.0`) — simulation engine; no UI dependencies.
- `TheGameOfLife` (`net8.0-windows`) — Windows Forms app.
- `TheGameOfLife.Tests` (`net8.0`) — xUnit tests for the engine.

## Requirements

The .NET 8 SDK (or Visual Studio 2022+). The app project uses
`System.Windows.Forms`, so it builds and runs on **Windows only**. The Core
library and its tests target plain `net8.0` and run on any platform.

## Build, run & test

```
dotnet test  TheGameOfLife/TheGameOfLife.sln        # engine tests (cross-platform)
dotnet build TheGameOfLife/TheGameOfLife.sln        # full build (Windows only)
dotnet run   --project TheGameOfLife/TheGameOfLife  # run the app (Windows only)
```

Or open `TheGameOfLife/TheGameOfLife.sln` in Visual Studio.
