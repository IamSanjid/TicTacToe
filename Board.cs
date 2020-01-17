using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TicTacToe
{

    public static class Extentions
    {
        public static bool IsVerticallyMatched (this char[,] state, char check_char, int depth, int startX = 0, int startY = 0)
        {
            bool result = false;
            int sum = 0;

            for (int y = startY, max_y = state.GetLength(0); y < max_y; y++)
            {
                for (int x = startX, max_x = state.GetLength(1); x < max_x; x++)
                {
                    if (state[y, x] == check_char)
                    {
                        for (int lame_y = y, count = 0; lame_y < max_y && count < depth; lame_y++, count++)
                        {
                            if (state[lame_y, x] == check_char)
                                sum++;
                            else
                                break;
                            if (sum == depth) { result = true; goto RES; }
                        }
                        sum = 0;
                    }
                }
            }
            RES:
            return result;
        }

        public static bool IsHorizontallyMatched (this char[,] state, char check_char, int depth, int startX = 0, int startY = 0)
        {
            bool result = false;
            int sum = 0;

            for (int y = startY, max_y = state.GetLength(0); y < max_y; y++)
            {
                for (int x = startX, max_x = state.GetLength(1); x < max_x; x++)
                {
                    if (state[y, x] == check_char)
                    {
                        for (int lame_x = x, count = 0; lame_x < max_x && count < depth; lame_x++, count++)
                        {
                            if (state[y, lame_x] == check_char)
                                sum++;
                            else
                                break;
                            if (sum == depth) { result = true; goto RES; }
                        }
                        sum = 0;
                    }
                }
            }

            RES:
            return result;
        }

        public static bool IsDiagonallyMatched (this char[,] state, char check_char, int depth, int startX = 0, int startY = 0)
        {
            bool result = false;

            int sum = 0;

            int max_x = state.GetLength(1), max_y = state.GetLength(0);

            int check_max_x = max_x + (max_x % 2 == 0 ? 0 : 1);
            int check_max_y = max_y + (max_y % 2 == 0 ? 0 : 1);

            for (int y = startY; y < max_y; y++)
            {
                if (y >= check_max_y - depth)
                {
                    goto RET;
                }
                for (int x = startX; x < max_x; x++)
                {
                    if (state[y, x] == check_char)
                    {
                        int lame_inc = x >= check_max_x / 2 ? -1 : 1;
                        for (int lame_x = x, lame_y = y, count = 0; 
                            lame_x < max_x && lame_y < max_y && lame_x >= 0 && count < depth; 
                            lame_x += lame_inc, lame_y++, count++)
                        {
                            if (state[lame_y, lame_x] == check_char)
                                sum++;
                            else
                                break;
                            if (sum == depth) { result = true; goto RET; }
                        }
                        sum = 0;
                    }
                }
            }

            RET:
            return result;
        }

        public static Kind CheckMatchResult(this Board board)
        {
            var state = board.GetState();
            foreach(var player in board.Players)
            {
                var hori = state.IsHorizontallyMatched((char)player.Type, board.MatchDepth);
                var verti = state.IsVerticallyMatched((char)player.Type, board.MatchDepth);
                var diognally = state.IsDiagonallyMatched((char)player.Type, board.MatchDepth);
                if (
                    //player.IsWinner(board)
                    hori ||
                    verti ||
                    diognally
                )
                { 
                    return player.Type; 
                }
            }
            if (board.TotalMoves >= state.Length) 
                return Kind.Tie;
            return Kind.None;
        }

        public static IList<T> Clone<T>(this IList<T> listToClone) where T: ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }

        public static void Fill<T>(this T[,] array, T value)
        {
            for (int upper = 0, max_upper = array.GetLength(0), max_lower = array.GetLength(1); upper < max_upper; upper++)
            {
                for (int lower = 0; lower < max_lower; lower++)
                {
                    array[upper, lower] = value;
                }
            }
        }
    }

    public class Board
    {
        public List<Player> Players;

        public int DimensionX { get; private set; }
        public int DimensionY { get; private set; }

        public int MatchDepth => DimensionX > 4 && DimensionY > 4 ? 4 : 3;

        public Player CurrentPlayer => Players[_currentPlayerIndex];
        private int _currentPlayerIndex = 0;

        public int TotalMoves => _moves.Count;

        private List<(Kind type, int x, int y)> _moves;

        private readonly char[,] _state;

        private Difficulty _difficulty;
        int AI_Depth;

        public Action<Kind> MatchResultOut;
        public Action<long> AIDebuggingFinished;

        public bool DebugAITime = false;

        public Board(int dimensionX, int dimensionY, Difficulty difficulty)
        {
            _difficulty = difficulty;
            DimensionX = dimensionX;
            DimensionY = dimensionY;
            _moves = new List<(Kind type, int x, int y)>();
            Players = new List<Player>();
            _state = new char[dimensionY, dimensionX];

            if (difficulty == Difficulty.Easy)
                AI_Depth = 3;
            else if (difficulty == Difficulty.Medium)
                AI_Depth = 2;
            else
                AI_Depth = 0;

            _state.Fill(' ');
        }

        public void SetPlayers(IList<Kind> kinds)
        {
            Players.Clear();
            foreach(var kind in kinds)
                Players.Add(new Player(kind));
            _state.Fill(' ');
        }

        public char[,] GetState()
        {
            return _state;
        }

        private void NextPlayer()
        {
            CurrentPlayer.SetMove();
            _moves.Add((CurrentPlayer.Type, CurrentPlayer.X, CurrentPlayer.Y));
            
            if (_currentPlayerIndex >= Players.Count - 1)
            {
                _currentPlayerIndex = 0;
            }
            else
            {
                _currentPlayerIndex++;
            }
        }

        public bool SetMove(bool normalMove = true)
        {
            var res = this.CheckMatchResult();

            if (!normalMove && res == Kind.None)
            {
                var stopwatch = new Stopwatch();
                if (DebugAITime)
                {
                    stopwatch.Start();
                }
                
                var move = MinimaxAlgo.BestMove(this.Clone(), AI_Depth);

                if (DebugAITime)
                {
                    stopwatch.Stop();
                    AIDebuggingFinished?.Invoke(stopwatch.ElapsedMilliseconds);
                }
                
                if (move.x == -1) 
                    goto RES;
                CurrentPlayer.X = move.x;
                CurrentPlayer.Y = move.y;
                if (SetMove(true))
                    goto RES;
                return false;
            }

            if (_state[CurrentPlayer.Y, CurrentPlayer.X] != ' ') return false;

            _state[CurrentPlayer.Y, CurrentPlayer.X] = (char)CurrentPlayer.Type;
            NextPlayer();

            RES:

            res = this.CheckMatchResult();
            if (res != Kind.None)
            {
                MatchResultOut?.Invoke(res);
                return false;
            }

            return true;
        }

        public void UndoLastMove()
        {
            if (_moves.Count <= 0) return;
            
            if (_currentPlayerIndex >= Players.Count - 1)
            {
                _currentPlayerIndex = 0;
            }
            else if (_currentPlayerIndex <= 0)
            {
                _currentPlayerIndex = Players.Count - 1;
            }
            else
            {
                _currentPlayerIndex--;
            }

            var lastMove = _moves[_moves.Count - 1];            
            _state[lastMove.y, lastMove.x] = ' ';

            CurrentPlayer.RemoveMove(lastMove.x, lastMove.y);
            
            _moves.Remove(lastMove);
        }

        public Board Clone()
        {
            var board = new Board(DimensionX, DimensionY, _difficulty);
            Buffer.BlockCopy(this._state, 0, board._state, 0, this._state.Length * sizeof(char));

            board.Players = (List<Player>)this.Players.Clone();
            board._moves = this._moves.ToList();

            board._currentPlayerIndex = _currentPlayerIndex;

            return board;
        }

        public bool SetMove(int x, int y)
        {
            if (_state[y, x] != ' ') 
                return false;
            CurrentPlayer.X = x;
            CurrentPlayer.Y = y;
            _state[y, x] = (char)CurrentPlayer.Type;
            NextPlayer();
            return true;
        }
    }
}