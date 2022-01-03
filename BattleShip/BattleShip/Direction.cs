using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleShip
{
    public class Direction
    {
        public enum DirectionType
        {
            North,
            South,
            West,
            East
        }

        public enum TurningPoint
        {
            Horizontal,
            Vertical
        }
    }
}
