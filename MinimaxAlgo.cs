using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TicTacToe
{
    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }
    public class MinimaxAlgo
    {
        private class Moves
        {
            public int BestScore = Int32.MinValue;
            public Dictionary<Tuple<int, int>, int> BestMoves;
            public List<Tuple<int, int>> WorseMoves;
        }

        private static Random Random = new Random();

        async private static Task<Moves> GetMoves(Board _currentBoard, int depth)
        {
            int bestScore = int.MinValue;

            var best_moves = new Dictionary<Tuple<int, int>, int>();
            var worse_moves = new List<Tuple<int, int>>();

            await Task.Run(() => {
                for (int y = 0, max_y = _currentBoard.GetMoves().GetLength(0); y < max_y; y++)
                {
                    for (int x = 0, max_x = _currentBoard.GetMoves().GetLength(1); x < max_x; x++)
                    {
                        if (_currentBoard.GetMoves()[y, x] == ' ')
                        {
                            _currentBoard.SetMove(x, y, (char)_currentBoard.CurrentPlayer.Type);

                            int score = MinMax(_currentBoard.Clone(), false, int.MinValue, int.MaxValue, depth);
                            if (score >= bestScore)
                            {
                                best_moves.Add(new Tuple<int, int>(x, y), score);
                                bestScore = score;
                            }
                            else
                                worse_moves.Add(new Tuple<int, int>(x, y));

                            _currentBoard.UndoLastMove();
                        }
                    }
                }
            });
            
            return new Moves{ BestMoves = best_moves, WorseMoves = worse_moves, BestScore = bestScore };
        }

        async public static Task<Tuple<int, int>> BestMove(Board _currentBoard, int depth)
        {
            var moves = await GetMoves(_currentBoard, depth);
            var move = new Tuple<int, int>(-1, -1);
            var best_moves = moves.BestMoves.Keys.ToList();
            foreach(var mv in moves.BestMoves)
            {
                if (mv.Value != moves.BestScore)
                    best_moves.Remove(mv.Key);
            }
            if (best_moves.Count > 0)
                move = best_moves[Random.Next(best_moves.Count)];
            else
                move = moves.WorseMoves.FirstOrDefault();
            return move;
        }

        
        private static int MinMax(Board _currentBoard, bool isMaximizingPlayer, int alpha, int beta, int depth)
        {
            var res = _currentBoard.CheckMatchResult();
            if (res != MatchResult.None || depth == 0)
            {
                if (res == MatchResult.Tie) return 0;
                if (isMaximizingPlayer && 
                    res.ToString().ToLower()[0] == (char)_currentBoard.CurrentPlayer.Type)
                {
                    return 1;
                }
                else if (!isMaximizingPlayer && res.ToString().ToLower()[0] != (char)_currentBoard.CurrentPlayer.Type)
                    return 1;
                else
                    return -1;
            }

            if (isMaximizingPlayer)
            {
                int bestScore = Int32.MinValue;
                for (int y = 0, max_y = _currentBoard.GetMoves().GetLength(0); y < max_y; y++)
                {
                    for (int x = 0, max_x = _currentBoard.GetMoves().GetLength(1); x < max_x; x++)
                    {
                        if (_currentBoard.GetMoves()[y, x] == ' ')
                        {
                            _currentBoard.SetMove(x, y, (char)_currentBoard.CurrentPlayer.Type);
                            
                            int score = MinMax(_currentBoard.Clone(), false, alpha, beta, depth - 1);
                            bestScore = Math.Max(bestScore, score);
                            alpha = Math.Max(alpha, score);
                            
                            if (beta <= alpha)
                                break;
                            
                            _currentBoard.UndoLastMove();
                        }
                    }
                }
                return bestScore;
            }
            else
            {
                int bestScore = Int32.MaxValue;
                for (int y = 0, max_y = _currentBoard.GetMoves().GetLength(0); y < max_y; y++)
                {
                    for (int x = 0, max_x = _currentBoard.GetMoves().GetLength(1); x < max_x; x++)
                    {
                        if (_currentBoard.GetMoves()[y, x] == ' ')
                        {
                            _currentBoard.SetMove(x, y, (char)_currentBoard.CurrentPlayer.Type);
                            
                            int score = MinMax(_currentBoard.Clone(), true, alpha, beta, depth - 1);
                            bestScore = Math.Min(bestScore, score);
                            beta = Math.Min(beta, score);

                            if (beta <= alpha)
                                break;

                            _currentBoard.UndoLastMove();
                        }
                    }
                }
                return bestScore;
            }
        }
    }
}