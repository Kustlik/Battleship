using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleShip
{
    public class Ship
    {
        public enum shipFilterEnum //Ship categories
        {
            Carrier,
            Battleship,
            Cruiser,
            Submarine,
            Destroyer
        }

        private int length;
        public List<(int, int)> Coordinates { get; set; }
        public List<(int, int)> DestroyedParts { get; set; }

        public Ship(shipFilterEnum shipFilter)
        {
            Coordinates = new List<(int, int)>();
            DestroyedParts = new List<(int, int)>();

            switch (shipFilter)
            {
                case shipFilterEnum.Carrier:
                    length = 5;
                    break;
                case shipFilterEnum.Battleship:
                    length = 4;
                    break;
                case shipFilterEnum.Cruiser:
                    length = 3;
                    break;
                case shipFilterEnum.Submarine:
                    length = 3;
                    break;
                case shipFilterEnum.Destroyer:
                    length = 2;
                    break;
            }
        }

        public int GetLength()
        {
            return length;
        }
    }
}
