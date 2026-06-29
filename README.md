# the-game-of-life
A Windows Forms desktop implementation of Conway's Game of Life, built in C# on .NET Core 3.1.

The simulation is rendered by writing pixels directly into a `Bitmap` (via `unsafe` `LockBits` and `Parallel.For`) and displaying it in a `PictureBox`. The grid update applies the standard B3/S23 rules in two parallel passes.

## Requirements

Windows with the .NET Core 3.1 SDK (or Visual Studio 2019+). The project uses `System.Windows.Forms`, so it only builds and runs on Windows.

## Build & run

```
dotnet build TheGameOfLife/TheGameOfLife.sln
dotnet run --project TheGameOfLife/TheGameOfLife
```

Or open `TheGameOfLife.sln` in Visual Studio.
