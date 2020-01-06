using System;
using System.Text;

namespace TicTacToe
{
    class Program
    {
        static Board _board;
        static Random Random = new Random();

        static int mode = 2;
        private static bool isGameOver = false;

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
            Console.Clear();
            ClearBoard();
            Console.SetCursorPosition(0, 0);
            if (mode is 1 && (Random.Next(2) + 1) == 2) _board.SetMove(true, true);
        }

        static void Main(string[] args)
        {
            START:
            ShowMenu();
            ShowMove();

            bool run = true;
            while(run)
            {
                var keyInfo = Console.ReadKey();
                
                switch(keyInfo.Key)
                {
                    case ConsoleKey.LeftArrow:
                        if (_board.CurrentPlayer.X > 0)
                            _board.CurrentPlayer.X--;
                        Console.CursorLeft = _board.CurrentPlayer.X * 2;
                        break;
                    case ConsoleKey.RightArrow:
                        if (_board.CurrentPlayer.X < _board.DimensionX - 1)
                            _board.CurrentPlayer.X++;
                        Console.CursorLeft = _board.CurrentPlayer.X * 2;
                        break;
                    case ConsoleKey.UpArrow:
                        if (_board.CurrentPlayer.Y > 0)
                            _board.CurrentPlayer.Y--;
                        Console.CursorTop = _board.CurrentPlayer.Y;
                        break;
                    case ConsoleKey.DownArrow:
                        if (_board.CurrentPlayer.Y < _board.DimensionY - 1)
                            _board.CurrentPlayer.Y++;
                        Console.CursorTop = _board.CurrentPlayer.Y;
                        break;
                    case ConsoleKey.Enter:
                        if (_board.SetMove(mode is 1) && !isGameOver)
                        {
                            ShowMove();
                        }
                        else if (isGameOver)
                        {
                            goto START;
                        }
                        break;
                }
            }
        }

        static void ShowMove(bool showPos = true)
        {
            Console.SetCursorPosition(0, 0);
            _board.ShowMove();
            if (showPos){
                Console.WriteLine($"Player {(char)_board.CurrentPlayer.Type}'s Move.");
                Console.CursorLeft = _board.CurrentPlayer.X * 2;
                Console.CursorTop = _board.CurrentPlayer.Y;
            }
        }

        static void ClearBoard(int tab = 0)
        {
            _board = Board.NewBoard(3, 3);
            //_board.Print(tab);
            _board.MatchResultOut += TheMatchResultOut;
        }

        static void TheMatchResultOut(MatchResult result)
        {
            Console.Clear();
            ShowMove(false);
            string msg = result != MatchResult.Tie ? "Player " + result.ToString() + " has won!" : "The match is a Tie";
            Console.WriteLine("The result is: \n\t" + msg);
            isGameOver = true;
        }
    }
}
