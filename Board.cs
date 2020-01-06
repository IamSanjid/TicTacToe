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

            for (int y = startY; y < depth; y++)
            {
                for (int x = startX; x < depth; x++)
                {
                    sum += moves[y, x];
                }
                if (sum == check_char * depth) { result = true; break; }
                sum = 0;
            }

            return result;
        }

        public static bool IsHorizontallyMatched (this char[,] moves, char check_char, int depth, int startX = 0, int startY = 0)
        {
            bool result = false;

            int sum = 0;

            for (int x = startX; x < depth; x++)
            {
                for (int y = startY; y < depth; y++)
                {
                    sum += moves[y, x];
                }
                if (sum == check_char * depth) { result = true; break; }
                sum = 0;
            }

            return result;
        }

        public static bool IsDiagonallyMatched (this char[,] moves, char check_char, int depth, int startX = 0, int startY = 0)
        {
            bool result = false;

            int sum = 0;

            for (int y = startY; y < depth; y++)
            {
                for (int x = startX; x < depth; x++)
                {
                    if (moves[y, x] == check_char)
                    {
                        int lame_x = x;
                        int lame_y = y;

                        int lame_inc = x <= depth / 2 ? 1 : -1;

                        for (; lame_x < depth && lame_y < depth; lame_x += lame_inc, lame_y++)
                        {
                            sum += moves[lame_x, lame_y];
                            if (sum == check_char * depth)
                            {
                                result = true;
                                goto RET;
                            }
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

        int AI_Depth = 9;

        public Action<MatchResult> MatchResultOut;

        public static Board NewBoard(int dimensionX = 3, int dimensionY = 3, Difficulty difficulty = Difficulty.Hard)
        {           
            Board _board = new Board();

            if (difficulty == Difficulty.Easy)
                _board.AI_Depth = 3;
            else if (difficulty == Difficulty.Medium)
                _board.AI_Depth = 4;

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

        async public Task<bool> SetMove(bool AI = true, bool fpAI = false)
        {
            if (fpAI && LastPlayer is null)
            {
                var move = await MinimaxAlgo.BestMove(this.Clone(), AI_Depth);
                if (move is null) goto RES;
                CurrentPlayer.X = move.Item1;
                CurrentPlayer.Y = move.Item2;
                await SetMove(false);
                return true;
            }

            if (_moves[CurrentPlayer.Y, CurrentPlayer.X] != ' ') return false;

            _moves[CurrentPlayer.Y, CurrentPlayer.X] = (char)CurrentPlayer.Type;
            NextPlayer();

            var res = this.CheckMatchResult();

            if (AI && res == MatchResult.None)
            {
                var move = await MinimaxAlgo.BestMove(this.Clone(), AI_Depth);
                if (move is null) goto RES;
                CurrentPlayer.X = move.Item1;
                CurrentPlayer.Y = move.Item2;
                await SetMove(false);
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
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    if (_moves[y, x] != ' ')
                    {
                        if (y < 2)
                            _mainBoard.Append(UNDERLINE + _moves[y, x] + RESET);
                        else
                            _mainBoard.Append(_moves[y, x]);
                    }
                    else
                    {
                        _mainBoard.Append(y < 2 ? "_" : " ");
                    }

                    if (x < 2)
                    {
                        _mainBoard.Append("|");
                    }
                }

                if (y < 2)
                    _mainBoard.AppendLine();
            }

            for (int i = 0; i < tab; i++)
                Console.Write("\t");
            Console.WriteLine(_mainBoard.ToString().Split('\n')[0]);
            for (int i = 0; i < tab; i++)
                Console.Write("\t");
            Console.WriteLine(_mainBoard.ToString().Split('\n')[1]);
            for (int i = 0; i < tab; i++)
                Console.Write("\t");
            Console.WriteLine(_mainBoard.ToString().Split('\n')[2]);
        }

        public override string ToString()
        {
            return _mainBoard.ToString();
        }
    }
}