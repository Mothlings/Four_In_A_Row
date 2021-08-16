using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;


namespace FourInARow
{

    class FourInARowBoard
    {
        public int[,] GameBoard { get; private set; }

        const int EMPTY = 0;
        const int PLAYER = 1;
        const int AI = 2;
        const int ROWCOUNT = 6;
        const int COLUMNCOUNT = 7;
        public static List<string> txtList;
        public string txt = "";
        public bool isGameOver { get; private set; }

        public FourInARowBoard(int rows, int cols)
        {
            txtList = new List<string>();
            isGameOver = false;
            // Instantiate an empty board
            GameBoard = new int[rows, cols];
            for (int row = 0; row < this.GameBoard.GetLength(0); row++)
                for (int col = 0; col < this.GameBoard.GetLength(1); col++)
                    this.GameBoard[row, col] = 0;
        }

        public void DropPiece(FourInARowBoard GameBoard, int row, int column, int side)
        {
            GameBoard.GameBoard[row, column] = side;
        }

        public bool IsValidLocation(FourInARowBoard GameBoard, int column)
        {
            return (GameBoard.GameBoard[ROWCOUNT - 1, column] == 0); //if location is empty
        }

        public List<int> GetValidLocation(FourInARowBoard GameBoard)
        {
            List<int> validLocations = new List<int>();
            for (int i =0; i <7; i++)
            {
                if (GameBoard.GameBoard[5,i] == 0)
                {
                    validLocations.Add(i);
                }
            }
            return validLocations;
        }
        public int GetNextOpenRow(FourInARowBoard GameBoard, int column)
        {
            for (int row = 0; row < ROWCOUNT; row++)
            {
                if(GameBoard.GameBoard[row,column]== 0)
                {
                    return row;
                }
            }
            return -1;
        }

        public bool WinningMove(FourInARowBoard GameBoard, int side)
        {
            // horizontal check
            for (int col = 0; col < COLUMNCOUNT - 3; col++)
            {
                for (int row = 0; row < ROWCOUNT; row++)
                {
                    if (GameBoard.GameBoard[row, col] == side && GameBoard.GameBoard[row, col + 1] == side && GameBoard.GameBoard[row, col + 2] == side && GameBoard.GameBoard[row, col + 3] == side)
                    {
                        return true;
                    }
                }
            }

            // vertical check
            for (int col = 0; col < COLUMNCOUNT; col++)
            {
                for (int row = 0; row < ROWCOUNT - 3; row++)
                {
                    if (GameBoard.GameBoard[row, col] == side && GameBoard.GameBoard[row+1, col] == side && GameBoard.GameBoard[row+2, col] == side && GameBoard.GameBoard[row+3, col] == side)
                    {
                        return true;
                    }
                }
            }


            //diaganol pos int check
            for (int col = 0; col < COLUMNCOUNT - 3; col++)
            {
                for (int row = 0; row < ROWCOUNT - 3; row++)
                {
                    if (GameBoard.GameBoard[row, col] == side && GameBoard.GameBoard[row+1, col+1] == side && GameBoard.GameBoard[row+2, col+2] == side && GameBoard.GameBoard[row+3, col+3] == side)
                    {
                        return true;
                    }
                }
            }
            //diaganol neg int check
            for (int col = 0; col < COLUMNCOUNT - 3; col++)
            {
                for (int row = 3; row < ROWCOUNT; row++)
                {
                    if (GameBoard.GameBoard[row, col] == side && GameBoard.GameBoard[row-1, col+1] == side && GameBoard.GameBoard[row-2, col+2] == side && GameBoard.GameBoard[row-3, col+3] == side)
                    {
                        return true;
                    }
                }
            }
            return false;
        }


        public bool InsertPiece(int side, int column)
        {
            // Start from bottom, work up
            for (int row = GameBoard.GetLength(0) - 1; row >= 0; row--)
            {
                if (GameBoard[row, column] == 0)
                {
                    GameBoard[row, column] = side;
                    return true;
                }
            }
            return false;
        }


        public int CountPiecesInColumn(int column)
        {
            int pieceCount = 0;
            for (int row = GameBoard.GetLength(0) - 1; row >= 0; row--)
            {
                if (GameBoard[row, column] != 0)
                {
                    pieceCount++;
                }
            }
            return pieceCount;
        }


        public int WindoEval(List<int> window, int piece)
        {
            int score = 0;
            int oppPiece = 0;

            int countPiece = 0;
            int countOppPiece = 0;
            int countEmpty = 0;

            oppPiece = PLAYER;
            if(piece == PLAYER)
            {
                oppPiece = AI;
            }

            for (int i =0; i< window.Count(); i++)
            {
                if (window[i] == piece)
                {
                    countPiece++;
                }
                else if (window[i] == EMPTY)
                {
                    countEmpty++;
                }
                else if (window[i] == oppPiece)
                {
                    countOppPiece++;
                }
            }
            if (countPiece == 4)
            {
                score += 100;
            }
            else if (countPiece == 3 && countEmpty == 1)
            {
                score += 5;
            }
            else if (countPiece == 2 && countEmpty == 2)
            {
                score += 2;
            }
            if (countOppPiece == 3 && countEmpty == 1)
            {
                score -= 4;
            }

            return score;
        }

        public int ScorePosCalc(FourInARowBoard GameBoard, int side)
        {
            int score = 0;
            int count = 0;
            int centerCount=0;
            List<int> centerArray = new List<int>();
            List<int> windowArray = new List<int>();

            // Score center column
            for (int row = 0; row < ROWCOUNT; row++)
            {
                centerArray.Add(GameBoard.GameBoard[row, 3]);
                if(GameBoard.GameBoard[row, 3] == side)
                {
                    centerCount++;
                }
            }
            score += centerCount * 3;
            // Score Horizontal
            count = 0;
            for (int row = 0; row < ROWCOUNT; row++)
            {
                count = 0;
                for (int column = 0; column < 4; column++)
                {
                   for (int i = 0; i < 4; i++)
                    {
                        windowArray.Add(GameBoard.GameBoard[row, column + count]);
                        count++;
                    }
                    count = 0;
                    score += WindoEval(windowArray, side);
                }
            }
            // Score Vertical
            for (int column = 0; column < COLUMNCOUNT; column++)
            {
                count = 0;
                for (int row = 0; row < 3; row++)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        windowArray.Add(GameBoard.GameBoard[row + count, column]);
                        count++;
                    }
                    count = 0;
                    score += WindoEval(windowArray, side);
                }
            }
            //score slop Diagonals
            //pos
            for (int row = 0; row < 3; row++)
            {
                count = 0;
                for (int column = 0; column < 4; column++)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        windowArray.Add(GameBoard.GameBoard[row + count, column + count]);
                        count++;
                    }
                    count = 0;
                    score += WindoEval(windowArray, side);
                }
            }
            //neg
            for (int row = 0; row < 3; row++)
            {
                count = 0;
                for (int column = 0; column < 4; column++)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        windowArray.Add(GameBoard.GameBoard[row + 3 - count, column + count]);
                        count++;
                    }
                    count = 0;
                    score += WindoEval(windowArray, side);
                }
            }
            return score;
        }

        public bool IsTerminalNode(FourInARowBoard GameBoard)
        {
            if(GetValidLocation(GameBoard).Count == 0)
            {
                return true;
            }
            else if (WinningMove(GameBoard, PLAYER))
            {
                return true;
            }
            else if (WinningMove(GameBoard, PLAYER))
            {
                return true;
            }

            return false;

        }

        public double[] Minmax(FourInARowBoard GameBoard, int depth, double alpha, double beta, bool maximizingPlayer)
        {
            double[] array = new double[2];
            List<int> validLocations = new List<int>();

            validLocations = GetValidLocation(GameBoard);
            bool isTerminal = IsTerminalNode(GameBoard);
            
            if (depth == 0 || isTerminal)
            {
                if (isTerminal)
                {
                    if (WinningMove(GameBoard, AI)){
                        array[0] = EMPTY;
                        array[1] = 100000000000000;
                        return array;
                    }
                    else if (WinningMove(GameBoard, PLAYER))
                    {
                        array[0] = EMPTY;
                        array[1] = -100000000000000;
                        return array;
                    }
                    else // Game is over, no more valid moves
                    {
                        array[0] = EMPTY;
                        array[1] = 0;
                        return array;
                    }

                }
                else // Depth is zero
                {
                    array[0] = EMPTY;
                    array[1] = ScorePosCalc(GameBoard, AI);

                    return array;
                }
            }

            if (maximizingPlayer)
            {
                double value = -100000000000000;
                var random = new Random();
                FourInARowBoard newBoard = new FourInARowBoard(6, 7);
                double[] Holder = new double[2];
                double column = validLocations[random.Next(validLocations.Count)]; //random index selected -> random value from list
                int row;
                for (int col = 0; col < validLocations.Count; col++)
                {
                    row = GetNextOpenRow(GameBoard, col);
                    if (row != -1) {
                        newBoard = Copy(newBoard, GameBoard.GameBoard);
                        DropPiece(newBoard, row, col, AI);

                        //txt = txt + "row "+ row + ", column " + col + ", depth  "+ depth + ", Alpha " +alpha + ", Beta "+ beta;
                       // txtList.Add(txt);
                       // txt = "";

                        Holder = new double[2];
                        Holder = Minmax(newBoard, depth - 1, alpha, beta, false);
                        double newScore = Holder[1];
                        if (newScore > value)
                        {
                            value = newScore;
                            column = col;
                        }
                        if (value > alpha)
                        {
                            alpha = value;
                        }
                        if (alpha >= beta)
                        {
                            break;
                        }
                    }
                }
                array[0] = column;
                array[1] = value;
                txt = "AI player: column  " + column + ", value " + value + ", depth  " + depth + ", Alpha " + alpha + ", Beta " + beta;
                txtList.Add(txt);
                txt = "";
                return array;
            }
            else // Minimizing player
            {
                double value = 100000000000000;
                var random = new Random();
                FourInARowBoard newBoard = new FourInARowBoard(6, 7);
                double[] Holder = new double[2];
                double column = validLocations[random.Next(validLocations.Count)]; //random index selected -> random value from list
                int row;
                for (int col = 0; col < validLocations.Count; col++)
                {
                    row = GetNextOpenRow(GameBoard, col);
                    if(row != -1)
                    {
                        newBoard = Copy(newBoard, GameBoard.GameBoard);
                        DropPiece(newBoard, row, col, PLAYER);

                    //  txt = txt + "row " + row + ", column " + col + ", depth  " + depth + ", Alpha " + alpha + ", Beta " + beta;
                    //  txtList.Add(txt);
                    //    txt = "";

                        Holder = Minmax(newBoard, depth - 1, alpha, beta, true);
                        double newScore = Holder[1];
                        if (newScore < value)
                        {
                            value = newScore;
                            column = col;
                        }
                        if (value < beta)
                        {
                            beta = value;
                        }
                        if (alpha >= beta)
                        {
                            break;
                        }
                    }
                }
                txt = "User player: column  " + column + ", value " + value + ", depth  " + depth + ", Alpha " + alpha + ", Beta " + beta;
                txtList.Add(txt);
                txt = "";
                array[0] = column;
                array[1] = value;

                WriteToTxt(txtList, "WriteLines.txt");
                txtList = new List<string>();
                return array;
            }
        }

        public void WriteToTxt(List<string> txtList, string path)
        {
            // Set a variable to the Documents path.

            try
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(path, true)) //true means we dont want to overwrite file > add onto existing
                {
                    foreach (string line in txtList)
                    {
                        file.WriteLine(line+ "\r\n");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("This program did an opsie");
            }

        }

        public FourInARowBoard Copy(FourInARowBoard newBoard, int[,] updatedBoard)
        {
            for (int row = 0; row < updatedBoard.GetLength(0); row++)
                for (int col = 0; col < updatedBoard.GetLength(1); col++)
                    newBoard.GameBoard[row, col] = updatedBoard[row, col];
            return newBoard;
        }
    }
}