using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Threading;

namespace BattleShip
{
    public class Game
    {
        public MainWindow AppWindow { get; set; } //Main game window.
        public bool GameStarted { get; set; }
        public int WaveTimer { get; set; }
        public int StartDelay { get; set; }
        public double ProgressBarUpdateFrequency { get; set; }
        private Random random = new Random();
        private CancellationTokenSource cts;

        public Game()
        {
            AppWindow = (MainWindow)Application.Current.MainWindow;
            cts = new CancellationTokenSource();
            WaveTimer = (int)(AppWindow.WaveSlider.Value * 1000); //Convert to milliseconds(1000)
            StartDelay = (int)(AppWindow.StartDelaySlider.Value * 1000); //Convert to milliseconds(1000)
            ProgressBarUpdateFrequency = 60;

            NextWave(ChooseStartingPlayer());
        }

        private int ChooseStartingPlayer()
        {
            return random.Next(1, 3); //Random between player 1 and 2.
        }
        private async void NextWave(int playerNumber) //Wave exchange system.
        {
            AppWindow = (MainWindow)Application.Current.MainWindow;

            if (!GameStarted) //Delay after game start.
            {
                try
                {
                    await Task.Delay(StartDelay, cts.Token);
                }
                catch (Exception ex)
                {
                    return;
                }
                GameStarted = true;
            }

            if (playerNumber == 1)
            {
                var opponentBoard = AppWindow.GetPlayerBoard(2); //Player logical board.
                var opponentGrid = AppWindow.Player2Grid; //Player visual board.

                if (CheckWinCondition(opponentBoard)) //Show Winner bar if win condition is true.
                {
                    AppWindow.Player1WinnerBox.Visibility = Visibility.Visible;
                }
                else //Start progress bar to visualize wave.
                {
                    AppWindow.UpdateProgressBar(AppWindow.Player1ProgressBar, WaveTimer, ProgressBarUpdateFrequency);
                    try
                    {
                        await Task.Delay(WaveTimer, cts.Token);
                    }
                    catch (Exception ex)
                    {
                        return;
                    }
                }

                if (opponentBoard.MainPointOfInterest == (null, null)) //Attack random cell if not found anything on a board.
                {
                    HitRandomCell(opponentBoard, opponentGrid);
                }
                else //Attack Specific cell if found something.
                {
                    if (opponentBoard.MainPointOfInterest != opponentBoard.LastPointOfInterest)
                    {
                        CheckLastPointOfInterest(opponentBoard, opponentGrid);
                    }
                    else
                    {
                        CheckPointOfInterest(opponentBoard, opponentGrid);
                    }
                }

                if (!CheckWinCondition(opponentBoard)) //Show Winner bar if win condition is true, else continue normal exchange.
                {
                    NextWave(2);
                }
                else
                {
                    AppWindow.Player1WinnerBox.Visibility = Visibility.Visible;
                }
            }
            else if (playerNumber == 2)
            {
                var opponentBoard = AppWindow.GetPlayerBoard(1); //Player logical board.
                var opponentGrid = AppWindow.Player1Grid; //Player visual board.

                if (CheckWinCondition(opponentBoard)) //Show Winner bar if win condition is true.
                {
                    AppWindow.Player2WinnerBox.Visibility = Visibility.Visible;
                }
                else //Start progress bar to visualize wave.
                {
                    AppWindow.UpdateProgressBar(AppWindow.Player2ProgressBar, WaveTimer, ProgressBarUpdateFrequency);
                    try
                    {
                        await Task.Delay(WaveTimer, cts.Token);
                    }
                    catch (Exception ex)
                    {
                        return;
                    }
                }

                if (opponentBoard.MainPointOfInterest == (null, null)) //Attack random cell if not found anything on a board.
                {
                    HitRandomCell(opponentBoard, opponentGrid);
                }
                else //Attack Specific cell if found something.
                {
                    if (opponentBoard.MainPointOfInterest != opponentBoard.LastPointOfInterest)
                    {
                        CheckLastPointOfInterest(opponentBoard, opponentGrid);
                    }
                    else
                    {
                        CheckPointOfInterest(opponentBoard, opponentGrid);
                    }
                }

                if (!CheckWinCondition(opponentBoard)) //Show Winner bar if win condition is true, else continue normal exchange.
                {
                    NextWave(1);
                }
                else
                {
                    AppWindow.Player2WinnerBox.Visibility = Visibility.Visible;
                }
            }
            else
            {
                throw new ArgumentException(playerNumber.ToString());
            }
        }
        public void StopWaves()
        {
            cts.Cancel();
        }//Token to stop delay task.

        #region Board targeting and investigitation
        private bool InteligentTargeting(PlayerBoard playerBoard, int x, int y) //Targeting taking into consideration left on the board lengths of the ships and following found ships.
        {
            if (playerBoard.GetCells()[x][y] == PlayerBoard.cellFilter.Ship) //Setting point of interest of the found ship.
            {
                playerBoard.SetStartingPointOfInterest((x, y));
                return true;
            }
            else if(playerBoard.GetCells()[x][y] == PlayerBoard.cellFilter.Water) //Checking if any left ship would fit into remaining squares.
            {
                return CheckIfShipWouldFit(GetShortestShip(playerBoard), playerBoard, x, y);
            }
            else
            {
                throw new ArgumentException("Wrong cellFilter type: " + playerBoard.GetCells()[x][y].ToString());
            }
        }
        public void HitRandomCell(PlayerBoard playerBoard, Grid playerGrid)
        {
            int x;
            int y;

            do
            {
                x = random.Next(1, playerBoard.BoardWidth + 1);
                y = random.Next(1, playerBoard.BoardHeight + 1);
            }
            while (!IsCellValidToHit(playerBoard, playerGrid, x, y));

            ChangeCellState(playerBoard, playerGrid, x, y);
            AppWindow.UpdateSingleCell(playerBoard, playerGrid, x, y);
        }
        private void ChangeCellState(PlayerBoard playerBoard, Grid playerGrid, int x, int y) //Checking hitted cell and colouring whether cell is water or ship.
        {
            if (playerBoard.GetCells()[x][y] == PlayerBoard.cellFilter.Water)
            {
                playerBoard.SetCells(x, y, PlayerBoard.cellFilter.Miss);
            }
            else if (playerBoard.GetCells()[x][y] == PlayerBoard.cellFilter.Ship)
            {
                playerBoard.SetCells(x, y, PlayerBoard.cellFilter.Hit);
                TargetShip(playerBoard, playerGrid, x, y);
            }
        }
        private void CheckPointOfInterest(PlayerBoard playerBoard, Grid playerGrid) //Investigitation first hitted and not sunked cell found on a ship.
        {
            var randomTurningPoint = random.Next(0, 2); // 0-1 Horizontal or Vertical

            // Point of hitted and not sunken ship to investigate.
            int x = playerBoard.MainPointOfInterest.Item1.GetValueOrDefault();
            int y = playerBoard.MainPointOfInterest.Item2.GetValueOrDefault();

            switch (randomTurningPoint) // Randomizing direction of investigitation, else comes into consideration of both sides of axis are already checked.
                                        // Setting predicted axis for next investigitation point.
            {
                case 0: // If random is Horizontal
                    if (DirectionalFit(Direction.TurningPoint.Horizontal, GetShortestShip(playerBoard), playerBoard, x, y))
                    {
                        CheckHorizontalNearbyPoint(playerBoard, playerGrid, x, y);
                    }
                    else
                    {
                        CheckVerticalNearbyPoint(playerBoard, playerGrid, x, y);
                    }
                    break;
                case 1: // If random is Vertical
                    if (DirectionalFit(Direction.TurningPoint.Vertical, GetShortestShip(playerBoard), playerBoard, x, y))
                    {
                        CheckVerticalNearbyPoint(playerBoard, playerGrid, x, y);
                    }
                    else
                    {
                        CheckHorizontalNearbyPoint(playerBoard, playerGrid, x, y);
                    }
                    break;
            }
        }
        private void CheckLastPointOfInterest(PlayerBoard playerBoard, Grid playerGrid) //Looking for ship patch following predicted axis.
        {
            // Next point of hitted and not sunken ship to investigate.
            var x = playerBoard.LastPointOfInterest.Item1.GetValueOrDefault();
            var y = playerBoard.LastPointOfInterest.Item2.GetValueOrDefault();

            if (playerBoard.PredictedDirection == Direction.TurningPoint.Horizontal)
            {
                CheckHorizontalNearbyPoint(playerBoard, playerGrid, x, y);
            }
            else if (playerBoard.PredictedDirection == Direction.TurningPoint.Vertical)
            {
                CheckVerticalNearbyPoint(playerBoard, playerGrid, x, y);
            }
        }
        private void CheckVerticalNearbyPoint(PlayerBoard playerBoard, Grid playerGrid, int x, int y) //Scanning nearby points of actual point of interest, if ship is found, another point is set.
        {
            List<int> randomUpOrDown = new List<int>(); // One point into up or down.

            // Checking if nearby cells were already checked, if yes, don't add them.
            if (!playerBoard.CheckForScannedCell(x, y + 1))
            {
                randomUpOrDown.Add(1);
            }
            else if (!playerBoard.CheckForScannedCell(x, y - 1))
            {
                randomUpOrDown.Add(-1);
            }

            if(randomUpOrDown.Count > 0) // If at least one point not scanned was found, execute this path.
            {
                var randomDirection = randomUpOrDown[random.Next(randomUpOrDown.Count)];

                ChangeCellState(playerBoard, playerGrid, x, (y + randomDirection));
                AppWindow.UpdateSingleCell(playerBoard, playerGrid, x, (y + randomDirection));

                if (playerBoard.GetCells()[x][y + randomDirection] == PlayerBoard.cellFilter.Hit) // Set another point of interest and predicted direction, if cell was successfully hit.
                {
                    playerBoard.SetLastPointOfInterestAndDirection((x, y + randomDirection), Direction.TurningPoint.Vertical);
                }
                if (!playerBoard.CheckIfMainPointOfInterestIsStillValid()) // Check if first point of investigitated ship is already sunk.
                {
                    if (playerBoard.IsHittedShipOnBoard()) // If first point is sunk, scan board to find if any not examined hit cell was left.
                    {
                        playerBoard.MainPointOfInterest = playerBoard.ScanForHittedShip();
                    }
                    else // If not found, reset point of interest to start hitting randomly.
                    {
                        playerBoard.ResetPointOfInterest();
                    }
                }
            }
            else if (playerBoard.LastPointOfInterest == playerBoard.MainPointOfInterest) // handling never ending loop, where there is no other point on axis left
            {
                if (playerBoard.IsHittedShipOnBoard())
                {
                    playerBoard.ResetPointOfInterest();
                    playerBoard.MainPointOfInterest = playerBoard.ScanForHittedShip();
                    CheckPointOfInterest(playerBoard, playerGrid);
                }
                else
                {
                    playerBoard.ResetPointOfInterest();
                    HitRandomCell(playerBoard, playerGrid);
                }
            }
            else // if everything is checked, get back to first point.
            {
                playerBoard.LastPointOfInterest = playerBoard.MainPointOfInterest;
                CheckLastPointOfInterest(playerBoard, playerGrid);
            }
        }
        private void CheckHorizontalNearbyPoint(PlayerBoard playerBoard, Grid playerGrid, int x, int y) //Scanning nearby points of actual point of interest, if ship is found, another point is set.
        {
            List<int> randomUpOrDown = new List<int>(); // One point into left or right.

            // Checking if nearby cells were already checked, if yes, don't add them.
            if (!playerBoard.CheckForScannedCell(x + 1, y))
            {
                randomUpOrDown.Add(1);
            }
            else if (!playerBoard.CheckForScannedCell(x - 1, y))
            {
                randomUpOrDown.Add(-1);
            }

            if (randomUpOrDown.Count > 0) // If at least one point not scanned was found, execute this path.
            {
                var randomDirection = randomUpOrDown[random.Next(randomUpOrDown.Count)];

                ChangeCellState(playerBoard, playerGrid, (x + randomDirection), y);
                AppWindow.UpdateSingleCell(playerBoard, playerGrid, (x + randomDirection), y);

                if (playerBoard.GetCells()[x + randomDirection][y] == PlayerBoard.cellFilter.Hit) // Set another point of interest and predicted direction, if cell was successfully hit.
                {
                    playerBoard.SetLastPointOfInterestAndDirection((x + randomDirection, y), Direction.TurningPoint.Horizontal);
                }
                if (!playerBoard.CheckIfMainPointOfInterestIsStillValid()) // Check if first point of investigitated ship is already sunk.
                {
                    if (playerBoard.IsHittedShipOnBoard()) // If first point is sunk, scan board to find if any not examined hit cell was left.
                    {
                        playerBoard.MainPointOfInterest = playerBoard.ScanForHittedShip();
                    }
                    else // If not found, reset point of interest to start hitting randomly.
                    {
                        playerBoard.ResetPointOfInterest();
                    }
                }
            }
            else if (playerBoard.LastPointOfInterest == playerBoard.MainPointOfInterest) // handling never ending loop, where there is no other point on axis left
            {
                if (playerBoard.IsHittedShipOnBoard())
                {
                    playerBoard.ResetPointOfInterest();
                    playerBoard.MainPointOfInterest = playerBoard.ScanForHittedShip();
                    CheckPointOfInterest(playerBoard, playerGrid);
                }
                else
                {
                    playerBoard.ResetPointOfInterest();
                    HitRandomCell(playerBoard, playerGrid);
                }
            }
            else // if everything is checked, get back to first point.
            {
                playerBoard.LastPointOfInterest = playerBoard.MainPointOfInterest;
                CheckLastPointOfInterest(playerBoard, playerGrid);
            }
        }
        private bool IsCellValidToHit(PlayerBoard playerBoard, Grid playerGrid, int x, int y) //Check if cell wasn't already hit.
        {
            if (playerBoard.GetCells()[x][y] == PlayerBoard.cellFilter.Water || playerBoard.GetCells()[x][y] == PlayerBoard.cellFilter.Ship)
            {
                return InteligentTargeting(playerBoard, x, y);
            }
            else
            {
                return false;
            }
        }
        private void TargetShip(PlayerBoard playerBoard, Grid playerGrid, int x, int y) //Hit Ship and set destroyed points, if destroyed points and coordinates are equal, sunk it.
        {
            foreach (Ship ship in playerBoard.Ships) //Check every ship left on a board.
            {
                if(ship.Coordinates.FirstOrDefault(c => (c.Item1 == x && c.Item2 == y)) != default) //Add to destroyed parts found coordinates.
                {
                    ship.DestroyedParts.Add((x, y));
                }

                //If list sequence of coordinates and destroyed parts is equal, sunk it.
                if (Enumerable.SequenceEqual(ship.Coordinates.OrderBy(t => t.Item1), ship.DestroyedParts.OrderBy(t => t.Item1))) //horizontal check
                {
                    SunkShip(ship, playerBoard, playerGrid);
                    break;
                }
                else if (Enumerable.SequenceEqual(ship.Coordinates.OrderBy(t => t.Item2), ship.DestroyedParts.OrderBy(t => t.Item2))) //vertical check
                {
                    SunkShip(ship, playerBoard, playerGrid);
                    break;
                }
            }
        }
        private void SunkShip(Ship ship, PlayerBoard playerBoard, Grid playerGrid) //Change all cell states of an ship to sunken and remove it from a board.
        {
            foreach ((int, int) coordinate in ship.Coordinates)
            {
                playerBoard.SetCells(coordinate.Item1, coordinate.Item2, PlayerBoard.cellFilter.Sunk);
                AppWindow.UpdateSingleCell(playerBoard, playerGrid, coordinate.Item1, coordinate.Item2);
            }
           playerBoard.Ships.Remove(ship);
        }
        #endregion

        #region Fitting ship into board
        private int GetShortestShip(PlayerBoard playerBoard)
        {
            int shortestShip = 0;

            foreach (Ship ship in playerBoard.Ships)
            {
                if (shortestShip == 0)
                {
                    shortestShip = ship.GetLength();
                }
                else if (ship.GetLength() < shortestShip)
                {
                    shortestShip = ship.GetLength();
                }
            }

            return shortestShip;
        }
        private bool CheckIfShipWouldFit(int shortestShip, PlayerBoard playerBoard, int x, int y) //Check if shortest ship left on a board will find into targeted cell.
        {
            if(DirectionalFit(Direction.TurningPoint.Vertical, shortestShip, playerBoard, x, y) ||
               DirectionalFit(Direction.TurningPoint.Horizontal, shortestShip, playerBoard, x, y))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool DirectionalFit(Direction.TurningPoint direction, int shortestShip, PlayerBoard playerBoard, int x, int y) //Check all cells according to axis, to count cells in between.
        {
            int NotCheckedCells = 1; //Starting point, always 1.

            switch (direction)
            {
                case Direction.TurningPoint.Vertical:
                    {
                        for (int i = 1; i < shortestShip; i++)
                        {
                            if (!playerBoard.CheckForScannedCell(x, y + i))
                            {
                                NotCheckedCells++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        for (int i = 1; i < shortestShip; i++)
                        {
                            if(!playerBoard.CheckForScannedCell(x, y - i))
                            {
                                NotCheckedCells++;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    break;
                case Direction.TurningPoint.Horizontal:
                    {
                        for (int i = 1; i < shortestShip; i++)
                        {
                            if (!playerBoard.CheckForScannedCell(x + i, y))
                            {
                                NotCheckedCells++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        for (int i = 1; i < shortestShip; i++)
                        {
                            if (!playerBoard.CheckForScannedCell(x - i, y))
                            {
                                NotCheckedCells++;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    break;
            }

            if(NotCheckedCells >= shortestShip) //if there is enough empty cells to fit, return true.
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        private bool CheckWinCondition(PlayerBoard playerBoard)
        {
            if (playerBoard.Ships.Count == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
