using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TicTacToe
{
    class Program
    {
        static Board _board;
        static Random Random = new Random();

        static int mode = 2;
        private static bool isGameOver = false;

        static int XOffset = -1;
        static int YOffset = -1;

        const string UNDERLINE = "\x1B[4m";
        const string RESET = "\x1B[0m";

        static StringBuilder _mainBoard = new StringBuilder();

        static bool run = true;

        static void ShowMenu()
        {
            isGameOver = false;
            Console.WriteLine("\t~~Welcome to TicTacToe Shit Version!~~");
            Console.WriteLine("1. Player Vs CPU");
            Console.WriteLine("2. Player Vs Player");
            Console.WriteLine("Which mode you want to play?! (Anything else to quit)");
            var key = Console.ReadKey();
            
            if (key.KeyChar == '1')
                mode = 1;
            else if (key.KeyChar == '2')
                mode = 2;
            else
                Environment.Exit(0);
            ClearBoard();
            Console.SetCursorPosition(0, 0);
            if (mode is 1 && Random.NextDouble() <= 0.5) 
                _board.SetMove(false);
        }

        static void Main(string[] args)
        {
            START:
            ShowMenu();
            ShowMove();

            while(run)
            {
                var keyInfo = Console.ReadKey();
                
                switch(keyInfo.Key)
                {
                    case ConsoleKey.LeftArrow:
                        if (_board.CurrentPlayer.X > 0)
                            _board.CurrentPlayer.X--;
                        Console.CursorLeft = _board.CurrentPlayer.X * 2 + XOffset;
                        break;
                    case ConsoleKey.RightArrow:
                        if (_board.CurrentPlayer.X < _board.DimensionX - 1)
                            _board.CurrentPlayer.X++;
                        Console.CursorLeft = _board.CurrentPlayer.X * 2 + XOffset;
                        break;
                    case ConsoleKey.UpArrow:
                        if (_board.CurrentPlayer.Y > 0)
                            _board.CurrentPlayer.Y--;
                        Console.CursorTop = _board.CurrentPlayer.Y + YOffset;
                        break;
                    case ConsoleKey.DownArrow:
                        if (_board.CurrentPlayer.Y < _board.DimensionY - 1)
                            _board.CurrentPlayer.Y++;
                        Console.CursorTop = _board.CurrentPlayer.Y + YOffset;
                        break;
                    case ConsoleKey.Enter:
                        if (_board.SetMove(true) && !isGameOver)
                        {
                            if (mode is 1)
                            {
                                if (!_board.SetMove(false))
                                { 
                                    goto START;
                                }
                            }
                            ShowMove();
                        }
                        else if (isGameOver)
                        {
                            goto START;
                        }
                        else
                        {
                            Console.CursorLeft = _board.CurrentPlayer.X * 2 + XOffset;
                            Console.CursorTop = _board.CurrentPlayer.Y + YOffset;
                        }
                        break;
                    default:
                        ShowMove();
                        break;
                }
            }
        }

        static void ShowMove(bool showPos = true, int tab = 0)
        {
            Console.SetCursorPosition(0, 0);
            ClearLines(_board.DimensionY + 5);

            Print(tab);
            if (lastAITime != -1)
            {
                Console.WriteLine($"AI Took Time: {lastAITime}ms");
                lastAITime = -1;
            }
            if (showPos)
            {
                Console.WriteLine($"Player {(char)_board.CurrentPlayer.Type}'s Move.");
                Console.CursorLeft = _board.CurrentPlayer.X * 2 + XOffset;
                Console.CursorTop = _board.CurrentPlayer.Y + YOffset;
            }
        }

        static void ClearLines(int lines)
        {
            int lastY = Console.CursorTop == 0 ? 0 : Console.CursorTop - 1;
            while (lines > 0)
            {
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(new string(' ', Console.WindowWidth));
                //Console.CursorTop++;
                lines--;
            }
            Console.SetCursorPosition(0, lastY);
        }

        static void Print(int tab)
        {
            _mainBoard.Clear();
            var state = _board.GetState();
            for (int y = 0; y < _board.DimensionY; y++)
            {
                for (int x = 0; x < _board.DimensionX; x++)
                {
                    if (state[y, x] != ' ')
                    {
                        if (y < _board.DimensionY - 1)
                            _mainBoard.Append(UNDERLINE + state[y, x] + RESET);
                        else
                            _mainBoard.Append(state[y, x]);
                    }
                    else
                    {
                        _mainBoard.Append(y < _board.DimensionY - 1 ? "_" : " ");
                    }

                    if (x < _board.DimensionX - 1)
                    {
                        _mainBoard.Append("|");
                    }
                }

                if (y < _board.DimensionY - 1)
                    _mainBoard.AppendLine();
            }

            var lines =_mainBoard.ToString().Split('\n');
            
            YOffset = Console.CursorTop;
            foreach(var line in lines)
            {
                for (int i = 0; i < tab; i++)
                    Console.Write("\t");
                XOffset = Console.CursorLeft;
                Console.WriteLine(line);
            }
        }

        public static void NewBoard(int dimensionX = 3, int dimensionY = 3, Difficulty difficulty = Difficulty.Hard)
        {           
            _board = new Board(dimensionX, dimensionY, difficulty);
            _board.SetPlayers(new PlayerType[] { PlayerType.Type_X, PlayerType.Type_O });
            _board.MatchResultOut += TheMatchResultOut;
        }

        static void ClearBoard()
        {
            Console.SetCursorPosition(0, 0);
            Console.Clear();
            _mainBoard = new StringBuilder();
            NewBoard(3, 3);
            #if DEBUG
            _board.AIDebuggingFinished += AIDebuggingFinished;
            _board.DebugAITime = true;
            #endif
        }
#if DEBUG
        static long lastAITime = -1;
#endif
        static void AIDebuggingFinished(long milliseconds)
        {
            lastAITime = milliseconds;
        }

        static void TheMatchResultOut(MatchResult result)
        {
            ShowMove(false);            
            string msg = result != MatchResult.Tie ? "Player " + result.ToString() + " has won!" : "The match is a Tie";
            Console.WriteLine("The result is: \n\t" + msg);
            isGameOver = true;
        }
    }
}
