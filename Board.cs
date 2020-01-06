using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe
{
    public enum MatchResult
    {
        X,
        O,
        Tie,
        None
    }

    public static class Extentions
    {
        public static bool IsVerticallyMatched (this char[,] moves, char check_char, int depth, int startX = 0, int startY = 0)
        {
            bool result = false;
            int sum = 0;

            for (int y = startY, max_y = moves.GetLength(0); y < max_y; y++)
            {
                for (int x = startX, max_x = moves.GetLength(1); x < max_x; x++)
                {
                    if (moves[y, x] == check_char)
                    {
                        for (int lame_y = y, count = 0; lame_y < max_y; lame_y++, count++)
                        {
                            if (count > depth)
                                break;
                            if (moves[lame_y, x] == check_char)
                                sum++;
                            if (sum == depth) { result = true; break; }
                        }
                        sum = 0;
                    }
                }
            }

            return result;
        }

        public static bool IsHorizontallyMatched (this char[,] moves, char check_char, int depth, int startX = 0, int startY = 0)
        {
            bool result = false;
            int sum = 0;

            for (int y = startY, max_y = moves.GetLength(0); y < max_y; y++)
            {
                for (int x = startX, max_x = moves.GetLength(1); x < max_x; x++)
                {
                    if (moves[y, x] == check_char)
                    {
                        for (int lame_x = x, count = 0; lame_x < max_x; lame_x++, count++)
                        {
                            if (count > depth)
                                break;
                            if (moves[y, lame_x] == check_char)
                                sum++;
                            if (sum == depth) { result = true; break; }
                        }
                        sum = 0;
                    }
                }
            }

            return result;
        }

        public static bool IsDiagonallyMatched (this char[,] moves, char check_char, int depth, int startX = 0, int startY = 0)
        {
            bool result = false;

            int sum = 0;

            int max_x = moves.GetLength(1), max_y = moves.GetLength(0);

            int check_max_x = max_x + (max_x % 2 == 0 ? 0 : 1);
            int check_max_y = max_y + (max_y % 2 == 0 ? 0 : 1);

            for (int y = startY; y < max_y; y++)
            {
                for (int x = startX; x < max_x; x++)
                {
                    if (moves[y, x] == check_char && y < check_max_y - depth)
                    {
                        int lame_inc = x >= check_max_x / 2 ? -1 : 1;
                        for (int lame_x = x, lame_y = y, count = 0; lame_x < max_x && lame_y < max_y 
                            && lame_x >= 0; lame_x += lame_inc, lame_y++, count++)
                        {
                            if (count > depth) break;
                            if (moves[lame_y, lame_x] == check_char)
                                sum++;
                            if (sum == depth) { result = true; goto RET; }
                        }
                        sum = 0;
                    }
                }
            }

            RET:
            return result;
        }

        public static bool IsSpaceLeft(this char[,] moves)
        {
            bool result = false; 
            for (int y = 0, max_y = moves.GetLength(0); y < max_y; y++)
            {
                for (int x = 0, max_x = moves.GetLength(1); x < max_x; x++)
                {
                    if (moves[y, x] == ' ')
                    {
                        result = true;
                        goto RET;
                    }
                }
            }
            RET:
            return result;
        }

        public static MatchResult CheckMatchResult(this Board board)
        {
            foreach(var player in board.Players)
            {
                var hori = board.GetMoves().IsHorizontallyMatched((char)player.Type, board.MatchDepth);
                var verti = board.GetMoves().IsVerticallyMatched((char)player.Type, board.MatchDepth);
                var diognally = board.GetMoves().IsDiagonallyMatched((char)player.Type, board.MatchDepth);
                if (
                    hori ||
                    verti ||
                    diognally
                )
                { 
                    return player.Type == PlayerType.Type_X ? MatchResult.X : MatchResult.O; 
                }
            }
            if (!board.GetMoves().IsSpaceLeft()) return MatchResult.Tie;
            return MatchResult.None;
        }
    }

    public class Board
    {
        const string UNDERLINE = "\x1B[4m";
        const string RESET = "\x1B[0m";

        public List<Player> Players;

        public int DimensionX { get; private set; }
        public int DimensionY { get; private set; }

        public int MatchDepth => DimensionX > 3 || DimensionY > 3 ? 4 : 3;

        public Player CurrentPlayer {get; private set;}
        public Player LastPlayer {get; private set;}

        private StringBuilder _mainBoard = new StringBuilder();

        private char[,] _moves;

        int AI_Depth;

        public Action<MatchResult> MatchResultOut;

        public static Board NewBoard(int dimensionX = 3, int dimensionY = 3, Difficulty difficulty = Difficulty.Easy)
        {           
            Board _board = new Board();

            if (difficulty == Difficulty.Easy)
                _board.AI_Depth = 3;
            else if (difficulty == Difficulty.Medium)
                _board.AI_Depth = 4;
            else
                _board.AI_Depth = 0;

            _board.Players = new List<Player>();
            _board.Players.Add(new Player(PlayerType.Type_X));
            _board.Players.Add(new Player(PlayerType.Type_O));

            _board.CurrentPlayer = _board.Players[0];

            _board.DimensionX = dimensionX;
            _board.DimensionY = dimensionY;

            _board._moves = new char[dimensionX, dimensionY];

            for (int y = 0; y < dimensionY; y++)
            {
                for (int x = 0; x < dimensionX; x++)
                {
                    _board._moves[y, x] = ' ';
                }
            }

            return _board;
        }

        public char GetMove(int x, int y)
        {
            return _moves[y, x];
        }

        public char[,] GetMoves()
        {
            return _moves;
        }

        private void NextPlayer(bool set_last = true)
        {
            int index = Players.FindIndex(pl => pl.Type == (!set_last ? LastPlayer.Type : CurrentPlayer.Type));
            if (set_last)
                LastPlayer = Players[index];
            
            if (index >= Players.Count - 1)
            {
                CurrentPlayer = Players[0];
            }
            else
            {
                CurrentPlayer = Players[index + 1];
            }
        }

        public bool SetMove(bool AI = true, bool fpAI = false)
        {
            if (fpAI && LastPlayer is null)
            {
                var move = MinimaxAlgo.BestMove(this.Clone(), AI_Depth);
                if (move.x == -1) goto RES;
                CurrentPlayer.X = move.Item1;
                CurrentPlayer.Y = move.Item2;
                SetMove(false);
                return true;
            }

            if (_moves[CurrentPlayer.Y, CurrentPlayer.X] != ' ') return false;

            _moves[CurrentPlayer.Y, CurrentPlayer.X] = (char)CurrentPlayer.Type;
            NextPlayer();

            var res = this.CheckMatchResult();

            if (AI && res == MatchResult.None)
            {
                var move = MinimaxAlgo.BestMove(this.Clone(), AI_Depth);
                if (move.x == -1) goto RES;
                CurrentPlayer.X = move.Item1;
                CurrentPlayer.Y = move.Item2;
                SetMove(false);
            }

            RES:

            res = this.CheckMatchResult();
            if (res != MatchResult.None)
            {
                MatchResultOut?.Invoke(res);
                return false;
            }

            return true;
        }

        public void UndoLastMove()
        {
            if (LastPlayer is null) return;
            _moves[LastPlayer.Y, LastPlayer.X] = ' ';
            CurrentPlayer = LastPlayer;
            LastPlayer = null;
        }

        public Board Clone()
        {
            var board = new Board() { DimensionX = this.DimensionX, DimensionY = this.DimensionY };

            board._moves = new char[this.DimensionX, this.DimensionY];
            Array.Copy(this._moves, board._moves, this._moves.Length);

            board.Players = new List<Player>();
            int index = 0;
            foreach(var player in this.Players)
            {
                board.Players.Add(player.Clone());
                if (player == this.CurrentPlayer)
                {
                    board.CurrentPlayer = board.Players[index];
                }
                if (player == this.LastPlayer)
                {
                    board.LastPlayer = board.Players[index];
                }
                index++;
            }

            return board;
        }

        public void SetMove(int x, int y, char _char)
        {
            _moves[y, x] = _char;
            var type = _char == (char)PlayerType.Type_X ? PlayerType.Type_X : PlayerType.Type_O;
            LastPlayer = Players.Find(p => p.Type == type);
            LastPlayer.X = x;
            LastPlayer.Y = y;
            NextPlayer(false);
        }

        public void ShowMove(int tab = 0)
        {
            _mainBoard.Clear();
            Print(tab);
        }

        public void Print(int tab = 0)
        {
            for (int y = 0; y < DimensionY; y++)
            {
                for (int x = 0; x < DimensionX; x++)
                {
                    if (_moves[y, x] != ' ')
                    {
                        if (y < DimensionY - 1)
                            _mainBoard.Append(UNDERLINE + _moves[y, x] + RESET);
                        else
                            _mainBoard.Append(_moves[y, x]);
                    }
                    else
                    {
                        _mainBoard.Append(y < DimensionY - 1 ? "_" : " ");
                    }

                    if (x < DimensionX - 1)
                    {
                        _mainBoard.Append("|");
                    }
                }

                if (y < DimensionY - 1)
                    _mainBoard.AppendLine();
            }

            var lines =_mainBoard.ToString().Split('\n');

            foreach(var line in lines)
            {
                for (int i = 0; i < tab; i++)
                    Console.Write("\t");
                Console.WriteLine(line);
            }
        }

        public override string ToString()
        {
            return _mainBoard.ToString();
        }
    }
}