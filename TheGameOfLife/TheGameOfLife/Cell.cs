using System;
using System.Collections.Generic;

namespace TheGameOfLife
{
    public class Cell
    {
        public bool Alive { get; set; } = randomObj.Next(0, 100) == 0;

        public List<Cell> Neighbors { get; set; } = new List<Cell>();

        private static readonly Random randomObj = new Random();
    }
}
