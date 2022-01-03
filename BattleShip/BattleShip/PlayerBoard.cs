using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BattleShip
{
    public class PlayerBoard
    {
        public MainWindow AppWindow { get; set; }
        public Dictionary<int, string> CellToAlphabet = new Dictionary<int, string>()
        {
            {1, "A"},
            {2, "B"},
            {3, "C"},
            {4, "D"},
            {5, "E"},
            {6, "F"},
            {7, "G"},
            {8, "H"},
            {9, "I"},
            {10, "J"}
        }; //Converting x axis to alphabet, it's needed to reference cells on a visual grid.
        public enum cellFilter
        {
            Water,
            Miss,
            Ship,
            Hit,
            Sunk
        } //Cell state values.

        public int BoardWidth { get; set; }
        public int BoardHeight { get; set;}
        public int PlayerNumber { get; set; }
        public List<Ship> Ships { get; set; }
        public (int?,int?) MainPointOfInterest { get; set; } //Point of interest found by another player, to investigitate.
        public (int?, int?) LastPointOfInterest { get; set; } //Continuing of investigitation.
        public Direction.TurningPoint? PredictedDirection { get; set; } //Predicted direction of investigitation.

        List<List<cellFilter>> boardMatrix = new List<List<cellFilter>>(); //Logical board containing information of cell states.

        public PlayerBoard(int playerNumber)
        {
            AppWindow = (MainWindow)Application.Current.MainWindow;

            PlayerNumber = playerNumber;
            Ships = new List<Ship>();

            BoardWidth = 10;
            BoardHeight = 10;

            InitializeMatrixCells();
        }

        public void SetCells(int x, int y, cellFilter value)
        {
            boardMatrix[x][y] = value;
        }
        public List<List<cellFilter>> GetCells()
        {
            return boardMatrix;
        }

        #region Point of interest
        public void SetStartingPointOfInterest((int, int) startingPointOfInterest)
        {
            MainPointOfInterest = startingPointOfInterest;
            LastPointOfInterest = startingPointOfInterest;
        }
        public void ResetPointOfInterest()
        {
            MainPointOfInterest = (null, null);
            LastPointOfInterest = (null, null);
            PredictedDirection = null;
        }
        public void SetLastPointOfInterestAndDirection((int,int) newGuessPosition, Direction.TurningPoint turningPoint)
        {
            LastPointOfInterest = newGuessPosition;
            PredictedDirection = turningPoint;
        }
        public bool CheckIfMainPointOfInterestIsStillValid()
        {
            var x = MainPointOfInterest.Item1.GetValueOrDefault();
            var y = MainPointOfInterest.Item2.GetValueOrDefault();  

            if (GetCells()[x][y] != cellFilter.Sunk)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region Board scanning
        public bool CheckForScannedCell(int x, int y) //Scanned cell - already hit by another player.
        {
            if (0 < x && x <= BoardWidth && 
                0 < y && y <= BoardHeight)
            {
                if (boardMatrix[x][y] == cellFilter.Miss ||
                boardMatrix[x][y] == cellFilter.Hit ||
                boardMatrix[x][y] == cellFilter.Sunk)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true; //Treating Boundaries as scanned cells
            }
        } 
        public bool IsHittedShipOnBoard()
        {
            if (ScanForHittedShip() == (null, null))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public (int?, int?) ScanForHittedShip()
        {
            for (int x = 1; x <= BoardWidth; x++)
            {
                for (int y = 1; y <= BoardWidth; y++)
                {
                    if (boardMatrix[x][y] == cellFilter.Hit)
                    {
                        return (x, y);
                    }
                }
            }
            return (null, null);
        }
        #endregion

        #region Board matrix
        private List<List<cellFilter>> InitializeMatrixCells()
        {
            for (int x = 0; x <= BoardWidth; x++)
            {
                boardMatrix.Add(new List<cellFilter>());
                for (int y = 0; y <= BoardHeight; y++)
                {
                    boardMatrix[x].Add(cellFilter.Water);
                }
            }
            ResetPlayMatrix();

            return boardMatrix;
        } //Mapping logical board.
        public void ResetPlayMatrix()
        {
            for (int x = 1; x <= BoardWidth; x++)
            {
                for (int y = 1; y <= BoardHeight; y++)
                {
                    boardMatrix[x][y] = cellFilter.Water;
                }
            }
        }
        #endregion

        #region Ship placement
        public void PlaceAllShips()
        {
            foreach (Ship.shipFilterEnum shipFilter in Enum.GetValues(typeof(Ship.shipFilterEnum)))
            {
                Ship newShip = new Ship(shipFilter);
                Ships.Add(newShip);
                PlaceShip(newShip);
            }
        }
        private void PlaceShip(Ship ship)
        {
            Random random = new Random();
            int x;
            int y;
            Direction.DirectionType direction;

            do //random placement
            {
                x = random.Next(1, BoardWidth + 1);
                y = random.Next(1, BoardHeight + 1);
            }
            while (!CheckEmptySurroundings(ship, x, y, out direction) || boardMatrix[x][y] != cellFilter.Water);

            BuildShip(ship, x, y, direction);
        }
        private bool CheckEmptySurroundings(Ship ship, int x, int y, out Direction.DirectionType direction) //Check all directions of start building point, if ship fits, add direction and randomize it.
        {
            List<Direction.DirectionType> emptySpaces = new List<Direction.DirectionType>();

            if ((ship.GetLength() - 1) < y)
            {
                for (int i = 1; i < ship.GetLength(); i++)
                {
                    if (boardMatrix[x][y - i] == cellFilter.Water)
                    {
                        if (i == ship.GetLength() - 1)
                        {
                            emptySpaces.Add(Direction.DirectionType.North);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            if (y < (BoardHeight - ship.GetLength() - 1))
            {
                for (int i = 1; i < ship.GetLength(); i++)
                {
                    if (boardMatrix[x][y + i] == cellFilter.Water)
                    {
                        if (i == ship.GetLength() - 1)
                        {
                            emptySpaces.Add(Direction.DirectionType.South);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            if ((ship.GetLength() - 1) < x)
            {
                for (int i = 1; i < ship.GetLength(); i++)
                {
                    if (boardMatrix[x - i][y] == cellFilter.Water)
                    {
                        if (i == ship.GetLength() - 1)
                        {
                            emptySpaces.Add(Direction.DirectionType.West);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            if (x < (BoardWidth - ship.GetLength() - 1))
            {
                for (int i = 1; i < ship.GetLength(); i++)
                {
                    if (boardMatrix[x + i][y] == cellFilter.Water)
                    {
                        if (i == ship.GetLength() - 1)
                        {
                            emptySpaces.Add(Direction.DirectionType.East);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (emptySpaces.Count > 0)
            {
                Random random = new Random();
                direction = emptySpaces[random.Next(0, emptySpaces.Count)];
                return true;
            }
            direction = default;
            return false;
        }
        private void BuildShip(Ship ship, int x, int y, Direction.DirectionType direction) //Build ship following direction passed into this method.
        {
            switch (direction)
            {
                case Direction.DirectionType.North:
                    for(int i = 0; i < ship.GetLength(); i++)
                    {
                        boardMatrix[x][y - i] = cellFilter.Ship;
                        ship.Coordinates.Add((x, y - i));
                    }
                    break;
                case Direction.DirectionType.South:
                    for (int i = 0; i < ship.GetLength(); i++)
                    {
                        boardMatrix[x][y + i] = cellFilter.Ship;
                        ship.Coordinates.Add((x, y + i));
                    }
                    break;
                case Direction.DirectionType.East:
                    for (int i = 0; i < ship.GetLength(); i++)
                    {
                        boardMatrix[x + i][y] = cellFilter.Ship;
                        ship.Coordinates.Add((x + i, y));
                    }
                    break;
                case Direction.DirectionType.West:
                    for (int i = 0; i < ship.GetLength(); i++)
                    {
                        boardMatrix[x - i][y] = cellFilter.Ship;
                        ship.Coordinates.Add((x - i, y));
                    }
                    break;
            }
        }
        #endregion
    }
}
