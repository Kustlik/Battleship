using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BattleShip
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        PlayerBoard player1Board = new PlayerBoard(1);
        PlayerBoard player2Board = new PlayerBoard(2);
        public Game CurrentGame { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        public PlayerBoard GetPlayerBoard(int playerNumber)
        {
            if (playerNumber == 1)
            {
                return player1Board;
            }
            else if(playerNumber == 2)
            {
                return player2Board;
            }
            else
            {
                throw new ArgumentException(playerNumber.ToString());
            }
        }

        #region Buttons
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartGame();
        }
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            RestartGame();
        }
        private void StartGame()
        {
            CurrentGame = new Game();

            player1Board.PlaceAllShips();
            player2Board.PlaceAllShips();
            UpdateBoard(player1Board, Player1Grid);
            UpdateBoard(player2Board, Player2Grid);

            UnlockUi(false);
            SwitchButtons();
        }
        private void RestartGame()
        {
            CurrentGame.StopWaves();

            player1Board.ResetPlayMatrix();
            player1Board.ResetPointOfInterest();
            player1Board.Ships.Clear();
            player2Board.ResetPlayMatrix();
            player2Board.ResetPointOfInterest();
            player2Board.Ships.Clear();
            UpdateBoard(player1Board, Player1Grid);
            UpdateBoard(player2Board, Player2Grid);

            UnlockUi(true);
            ResetUiElements();
            SwitchButtons();
        }
        #endregion

        #region UIElements
        private void UnlockUi(bool unlock)
        {
            if (unlock)
            {
                StartDelaySlider.IsEnabled = true;
                WaveSlider.IsEnabled = true;
            }
            else
            {
                StartDelaySlider.IsEnabled = false;
                WaveSlider.IsEnabled = false;
            }
        }
        private void SwitchButtons()
        {
            StartButton.IsEnabled = !StartButton.IsEnabled;
            ResetButton.IsEnabled = !ResetButton.IsEnabled;
        }
        private void ResetUiElements()
        {
            Player1ProgressBar.Value = 100;
            Player2ProgressBar.Value = 100;

            Player1WinnerBox.Visibility = Visibility.Hidden;
            Player2WinnerBox.Visibility = Visibility.Hidden;
        }
        #endregion

        #region Updates
        public void UpdateBoard(PlayerBoard playerBoard, Grid playerGrid)
        {
            for (int x = 1; x <= playerBoard.BoardWidth; x++)
            {
                for (int y = 1; y <= playerBoard.BoardHeight; y++)
                {
                    UpdateSingleCell(playerBoard, playerGrid, x, y);
                }
            }
        }
        public void UpdateSingleCell(PlayerBoard playerBoard, Grid playerGrid, int xPos, int yPos) //Creating xaml x:name, to reference cells on grid.
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("P");
            sb.Append(playerBoard.PlayerNumber.ToString());
            sb.Append("Cell");
            sb.Append(playerBoard.CellToAlphabet[xPos]);
            sb.Append(yPos.ToString());

            var cellName = sb.ToString();

            Rectangle cell = (Rectangle)playerGrid.FindName(cellName);
            cell.Fill = ColourCell(playerBoard.GetCells()[xPos][yPos]);
        }
        public async void UpdateProgressBar(ProgressBar progressBar, int waveTime, double frequency)
        {
            progressBar.Value = 0;
            var tickPerSec = ConvertMSTimeToTickPerFrequency(frequency); //How fast Value will be incremented.
            var value = (tickPerSec * progressBar.Maximum) / waveTime; //Value which is incremented on progress bar.

            while(progressBar.Value < 100)
            {
                await Task.Delay((int)tickPerSec - 1); //Double casted to int and rounded to lower integer to synchronize bar filling.
                progressBar.Value += value;
            }
        }
        #endregion

        private double ConvertMSTimeToTickPerFrequency(double frequency)
        {
            return (1000 / frequency);
        }

        private SolidColorBrush ColourCell(PlayerBoard.cellFilter cellFilter)
        {
            switch (cellFilter)
            {
                case PlayerBoard.cellFilter.Water:
                    return (SolidColorBrush)new BrushConverter().ConvertFrom("#FF4259B1");
                case PlayerBoard.cellFilter.Miss:
                    return (SolidColorBrush)new BrushConverter().ConvertFrom("#FFFFDC3A");
                case PlayerBoard.cellFilter.Ship:
                    return (SolidColorBrush)new BrushConverter().ConvertFrom("#FF464646");
                case PlayerBoard.cellFilter.Hit:
                    return (SolidColorBrush)new BrushConverter().ConvertFrom("#FFEF8686");
                case PlayerBoard.cellFilter.Sunk:
                    return (SolidColorBrush)new BrushConverter().ConvertFrom("#FFFF0000");  
            }
            throw new ArgumentException();
        } //Colours to represent states on a play grid.
    }
}
