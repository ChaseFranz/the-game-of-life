using System;
using System.Collections.Generic;
using System.Text;

namespace TheGameOfLife
{
    public class Cell
    {
        public bool Alive { get; set; }

        public List<Cell> Neighbors { get; set; }

        // TEMP
        private static Random rand = new Random();

        public Cell()
        {
            Neighbors = new List<Cell>();
            if (rand.Next(0,20) == 0)
            {
                Alive = true;
            }
            else
            {
                Alive = false;
            }
        }
    }
}
