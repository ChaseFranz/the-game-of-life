using System;

namespace TheGameOfLife
{
    public class Cell
    {
        public bool Alive { get; set; } = randomObj.Next(0, 10) == 0;

        public bool KillCell { get; set; }

        private static readonly Random randomObj = new Random();
    }
}