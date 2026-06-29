using System;
using System.Collections.Generic;
using System.Linq;
using TheGameOfLife.Core;
using Xunit;

namespace TheGameOfLife.Tests
{
    public class GameEngineTests
    {
        /// <summary>Creates an engine on an all-dead grid so patterns can be set explicitly.</summary>
        private static GameEngine NewEmptyEngine(int rows, int columns)
        {
            var engine = new GameEngine(new GameConfig
            {
                Rows = rows,
                Columns = columns,
                InitialLiveProbability = 0.0
            });
            engine.StartGame();
            return engine;
        }

        private static void SetAlive(GameEngine engine, params (int Row, int Col)[] cells)
        {
            foreach (var (row, col) in cells)
            {
                engine.Cells[row, col].Alive = true;
            }
        }

        private static HashSet<(int, int)> LiveCells(GameEngine engine)
        {
            var live = new HashSet<(int, int)>();
            for (int r = 0; r < engine.Rows; r++)
            {
                for (int c = 0; c < engine.Columns; c++)
                {
                    if (engine.Cells[r, c].Alive)
                    {
                        live.Add((r, c));
                    }
                }
            }
            return live;
        }

        [Fact]
        public void Block_IsAStillLife()
        {
            var engine = NewEmptyEngine(4, 4);
            SetAlive(engine, (1, 1), (1, 2), (2, 1), (2, 2));

            engine.NextCycle();

            Assert.Equal(
                new HashSet<(int, int)> { (1, 1), (1, 2), (2, 1), (2, 2) },
                LiveCells(engine));
        }

        [Fact]
        public void Blinker_OscillatesBetweenVerticalAndHorizontal()
        {
            var engine = NewEmptyEngine(5, 5);
            SetAlive(engine, (1, 2), (2, 2), (3, 2)); // vertical bar

            engine.NextCycle();
            Assert.Equal(
                new HashSet<(int, int)> { (2, 1), (2, 2), (2, 3) }, // horizontal bar
                LiveCells(engine));

            engine.NextCycle();
            Assert.Equal(
                new HashSet<(int, int)> { (1, 2), (2, 2), (3, 2) }, // back to vertical
                LiveCells(engine));
        }

        [Fact]
        public void LoneCell_DiesFromUnderpopulation()
        {
            var engine = NewEmptyEngine(3, 3);
            SetAlive(engine, (1, 1));

            engine.NextCycle();

            Assert.Empty(LiveCells(engine));
        }

        [Fact]
        public void CellWithFourNeighbors_DiesFromOverpopulation()
        {
            var engine = NewEmptyEngine(5, 5);
            // Centre cell plus its four orthogonal neighbours: the centre has 4
            // live neighbours and must die.
            SetAlive(engine, (2, 2), (1, 2), (3, 2), (2, 1), (2, 3));

            engine.NextCycle();

            Assert.False(engine.Cells[2, 2].Alive);
        }

        [Fact]
        public void DeadCellWithThreeNeighbors_IsBorn()
        {
            var engine = NewEmptyEngine(4, 4);
            SetAlive(engine, (0, 0), (0, 1), (1, 0)); // (1,1) has exactly 3 neighbours

            engine.NextCycle();

            Assert.True(engine.Cells[1, 1].Alive);
        }

        [Fact]
        public void EmptyGrid_StaysEmpty()
        {
            var engine = NewEmptyEngine(6, 6);

            engine.NextCycle();

            Assert.Empty(LiveCells(engine));
        }

        [Fact]
        public void Grid_DoesNotWrapAroundEdges()
        {
            var engine = NewEmptyEngine(3, 3);
            // Three cells along the top edge. If the grid wrapped, the bottom row
            // would see them as neighbours; it must not.
            SetAlive(engine, (0, 0), (0, 1), (0, 2));

            Assert.Equal(0, engine.GetLivingNeighborCount(2, 1));

            engine.NextCycle();

            // No live cell should appear in the bottom row.
            Assert.DoesNotContain(LiveCells(engine), cell => cell.Item1 == 2);
        }

        [Fact]
        public void GetLivingNeighborCount_CountsAllEightNeighbors()
        {
            var engine = NewEmptyEngine(3, 3);
            SetAlive(engine,
                (0, 0), (0, 1), (0, 2),
                (1, 0), /* centre */ (1, 2),
                (2, 0), (2, 1), (2, 2));

            Assert.Equal(8, engine.GetLivingNeighborCount(1, 1));
        }

        [Fact]
        public void SameSeed_ProducesIdenticalInitialGrids()
        {
            var a = new GameEngine(new GameConfig { Rows = 20, Columns = 20, RandomSeed = 42 });
            var b = new GameEngine(new GameConfig { Rows = 20, Columns = 20, RandomSeed = 42 });

            a.StartGame();
            b.StartGame();

            Assert.Equal(LiveCells(a), LiveCells(b));
        }

        [Fact]
        public void Generation_StartsAtZeroAndIncrementsPerCycle()
        {
            var engine = NewEmptyEngine(3, 3);
            Assert.Equal(0, engine.Generation);

            engine.NextCycle();
            engine.NextCycle();

            Assert.Equal(2, engine.Generation);
        }

        [Fact]
        public void StartGame_RaisesGenerationAdvanced()
        {
            var engine = new GameEngine(new GameConfig { Rows = 3, Columns = 3, InitialLiveProbability = 0.0 });
            int raised = 0;
            engine.GenerationAdvanced += (_, _) => raised++;

            engine.StartGame();
            engine.NextCycle();

            Assert.Equal(2, raised);
        }

        [Theory]
        [InlineData(0, 5)]
        [InlineData(5, 0)]
        [InlineData(-1, 5)]
        public void Constructor_RejectsNonPositiveDimensions(int rows, int columns)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new GameEngine(new GameConfig { Rows = rows, Columns = columns }));
        }

        [Fact]
        public void Constructor_RejectsProbabilityOutOfRange()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new GameEngine(new GameConfig { Rows = 5, Columns = 5, InitialLiveProbability = 1.5 }));
        }

        [Fact]
        public void Constructor_RejectsNullConfig()
        {
            Assert.Throws<ArgumentNullException>(() => new GameEngine(null!));
        }
    }
}
