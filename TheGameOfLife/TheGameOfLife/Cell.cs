using System;
using System.Collections.Generic;
using System.Text;

namespace TheGameOfLife
{
    public class Cell
    {
        public bool Alive { get; set; }

        public List<Cell> Neighbors { get; set; }

        private static Random randomObj = new Random();

        public Cell()
        {
            Neighbors = new List<Cell>();
            if (randomObj.Next(0,100) == 0)
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
