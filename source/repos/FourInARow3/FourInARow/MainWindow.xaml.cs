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
using System.Timers;
using System.Windows.Threading;
using Microsoft.Win32;
using System.Threading;
using weka.classifiers;
using System.IO;

namespace FourInARow
{

    public partial class MainWindow : Window
    {
        const int circleSize = 80;
        private bool gameOver = false;
        private FourInARowBoard board;
        private DispatcherTimer animationTimer;
        private Ellipse currentCircle;

        private int currentpiece;
        private int currentColumn;

        private const int PLAYER = 1;
        private const int AI = 2;
        private const int ROWCOUNT = 6;
        private const int COLUMNCOUNT = 7;


        // AIPlayer ai;

        public MainWindow()
        {
            InitializeComponent();
            NewGame();

        }
        private void NewGame()
        {
            gameOver = false;
            board = new FourInARowBoard(6, 7);
            currentpiece = PLAYER;
            animationTimer = new DispatcherTimer();
            animationTimer.Interval = new TimeSpan(0, 0, 0, 0, 15);
            animationTimer.Start();
            GameCanvas.Children.Clear();
            DrawBackground();
            EnableAllInsertButtons();
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            NewGame();
        }

        private void InsertButton_Click(int column)
        {

           double[] ColumnValue = new double[2];
           int row = 0;
           if (currentpiece == PLAYER && gameOver==false)
           {
               if (board.IsValidLocation(board, column))
               {
                   row = board.GetNextOpenRow(board, column);
                   board.DropPiece(board, row, column, currentpiece);
                   if (board.WinningMove(board, currentpiece))
                   {
                       announceWinner(currentpiece);
                   }
                   currentColumn = column;
                   DrawCircle(currentpiece, column);

               }
                if(gameOver == false)
                {
                    currentpiece = (currentpiece == AI) ? PLAYER : AI;
                }


            }
            //Thread thread = new Thread(new ParameterizedThreadStart(AIplayer));
            //thread.Start(column, ColumnValue, row);
            if (currentpiece == AI && gameOver == false)
            {
                var t = new Thread(() => AIplayer(column, ColumnValue, row));
                t.Start();
            }
        }



       public void AIplayer(int column, double[] ColumnValue, int row)
        {
            //Thread.Sleep(1000);
            if (currentpiece == AI)
            {
                ColumnValue = board.Minmax(board, 5, -100000000000, 100000000000, true);
                int col = (int)ColumnValue[0];
                if (col == -1)
                {
                    col = column;
                }
                double minimaxScore = (int)ColumnValue[1];

                if (board.IsValidLocation(board, col))
                {
                    row = board.GetNextOpenRow(board, col);
                    board.DropPiece(board, row, col, currentpiece);
                    if (board.WinningMove(board, currentpiece))
                    {
                        announceWinner(currentpiece);
                    }
                    currentColumn = col;
                    DrawCircle(2, col);
                }
                currentpiece = (currentpiece == AI) ? PLAYER : AI;

                this.Dispatcher.Invoke(() =>
                {
                    StatusText.Text = String.Format("{0}'s Turn", currentpiece);
                });
            }
        }


        #region InsertButton_Click Methods
        private void InsertButton0_Click(object sender, RoutedEventArgs e)
        {
            InsertButton_Click(0);
        }

        private void InsertButton1_Click(object sender, RoutedEventArgs e)
        {
            InsertButton_Click(1);
        }

        private void InsertButton2_Click(object sender, RoutedEventArgs e)
        {
            InsertButton_Click(2);
        }

        private void InsertButton3_Click(object sender, RoutedEventArgs e)
        {
            InsertButton_Click(3);
        }

        private void InsertButton4_Click(object sender, RoutedEventArgs e)
        {
            InsertButton_Click(4);
        }

        private void InsertButton5_Click(object sender, RoutedEventArgs e)
        {
            InsertButton_Click(5);
        }

        private void InsertButton6_Click(object sender, RoutedEventArgs e)
        {
            InsertButton_Click(6);
        }
        #endregion

        private void DrawBackground()
        {
            for (int row = 0; row < board.GameBoard.GetLength(0); row++)
            {
                for (int column = 0; column < board.GameBoard.GetLength(1); column++)
                {
                    Rectangle square = new Rectangle();
                    square.Height = circleSize;
                    square.Width = circleSize;
                    square.Fill = (column % 2 == 0) ? Brushes.White : Brushes.LightGray;
                    Canvas.SetBottom(square, circleSize * row);
                    Canvas.SetRight(square, circleSize * column);
                    GameCanvas.Children.Add(square);
                }
            }
        }

        private void DrawCircle(int piece, int col)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate {
                Ellipse circle = new Ellipse();
                circle.Height = circleSize;
                circle.Width = circleSize;
                circle.Fill = (piece == 1) ? Brushes.Red : Brushes.Black;
                Canvas.SetTop(circle, 0);
                Canvas.SetLeft(circle, col * 80);
                GameCanvas.Children.Add(circle);
                currentCircle = circle;
                int dropLength = circleSize * (board.GameBoard.GetLength(1) - 1 - board.CountPiecesInColumn(currentColumn));

                Canvas.SetTop(currentCircle, dropLength);
                GameCanvas.UpdateLayout();
            });
           
        }

        private void announceWinner(int piece)
        {

            if (piece != 0)
            {
                this.Dispatcher.Invoke(() =>
                {
                    StatusText.Text = String.Format("{0} player Wins!", currentpiece);
                });
                gameOver = true;
                DisableAllInsertButtons();
            }
        }

        private void DisableAllInsertButtons()
        {
            this.Dispatcher.Invoke(() =>
            {
                InsertButton0.IsEnabled = false;
                InsertButton1.IsEnabled = false;
                InsertButton2.IsEnabled = false;
                InsertButton3.IsEnabled = false;
                InsertButton4.IsEnabled = false;
                InsertButton5.IsEnabled = false;
                InsertButton6.IsEnabled = false;
            });
        }

        private void EnableAllInsertButtons()
        {
            InsertButton0.IsEnabled = true;
            InsertButton1.IsEnabled = true;
            InsertButton2.IsEnabled = true;
            InsertButton3.IsEnabled = true;
            InsertButton4.IsEnabled = true;
            InsertButton5.IsEnabled = true;
            InsertButton6.IsEnabled = true;
        }

  
    }
}