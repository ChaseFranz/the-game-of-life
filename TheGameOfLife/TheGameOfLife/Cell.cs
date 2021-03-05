using System;

namespace TheGameOfLife
{
    public class Cell
    {
        public bool Alive { get; set; } = randomObj.Next(0, 100) == 0;

        private static readonly Random randomObj = new Random();

    }
}