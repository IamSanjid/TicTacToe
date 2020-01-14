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
            public (int score, int depth) BestResult = (Int32.MinValue, Int32.MaxValue);
            public Dictionary<(int x, int y), (int score, int depth)> BestMoves;
            public List<(int x, int y)> WorseMoves;
        }

        private static Random Random = new Random();
        private static char AI_type = 'x';

        private static Moves GetMoves(Board _currentBoard, int depth)
        {
            //int bestScore = int.MinValue;
            //int minDepth = int.MaxValue;
            var bestResult = (score: Int32.MinValue, depth: Int32.MaxValue);

            var best_moves = new Dictionary<(int x, int y), (int score, int depth)>();
            var worse_moves = new List<(int x, int y)>();
            AI_type = (char)_currentBoard.CurrentPlayer.Type;

            int max_x = _currentBoard.GetState().GetLength(1), max_y = _currentBoard.GetState().GetLength(0);

            for (int y = 0; y < max_y; y++)
            {
                for (int x = 0; x < max_x; x++)
                {
                    if (_currentBoard.SetMove(x, y))
                    {
                        var result = MinMax(_currentBoard.Clone(), false, int.MinValue, int.MaxValue, depth);
                        _currentBoard.UndoLastMove();

                        if ((result.score >= bestResult.score)
                           || (result.score == bestResult.score && result.depth < bestResult.depth))
                        {
                            best_moves.Add((x, y), result);
                            bestResult = result;
                        }
                        else
                            worse_moves.Add((x, y));
                    }
                }
            }
            
            return new Moves{ BestMoves = best_moves, WorseMoves = worse_moves, BestResult = bestResult };
        }

        public static (int x, int y) BestMove(Board _currentBoard, int depth)
        {
            var moves = GetMoves(_currentBoard, depth);
            var move = (-1, -1);
            var best_moves = moves.BestMoves.Keys.ToList();
            foreach(var mv in moves.BestMoves)
            {
                if (mv.Value.score != moves.BestResult.score 
                    || (mv.Value.score == moves.BestResult.score && mv.Value.depth > moves.BestResult.depth))
                    best_moves.Remove(mv.Key);
            }
            if (best_moves.Count > 0)
                move = best_moves[Random.Next(best_moves.Count)];
            return move;
        }

        
        private static (int score, int depth) MinMax(Board _currentBoard, bool isMaximizingPlayer, int alpha, int beta, int depth)
        {
            var res = _currentBoard.CheckMatchResult();
            if (res != MatchResult.None)
            {
                var result = (score: -10, depth: depth);
                var winner = res.ToString().ToLowerInvariant()[0];
                if (res == MatchResult.Tie) 
                    result.score = 0;
                else if (winner == AI_type)
                {
                    result.score = 10;
                }

                
                return result;
            }

            int max_x = _currentBoard.GetState().GetLength(1);
            int max_y = _currentBoard.GetState().GetLength(0);

            if (isMaximizingPlayer)
            {
                var bestResult = (score: Int32.MinValue, depth: Int32.MaxValue);

                for (int y = 0; y < max_y; y++)
                {
                    for (int x = 0; x < max_x; x++)
                    {
                        if (_currentBoard.SetMove(x, y))
                        {
                            var result = MinMax(_currentBoard.Clone(), false, alpha, beta, depth + 1);
                            _currentBoard.UndoLastMove();

                            bestResult = Max(result, bestResult);
                            alpha = Math.Max(alpha, result.score);

                            /*if (result.score > bestResult.score) {
                                bestResult.score = result.score;
                                bestResult.depth = result.depth;
                            } else if (result.score == bestResult.score && result.depth < bestResult.depth) {
                                bestResult.depth = result.depth;
                            }*/
                            
                            if (beta <= alpha)
                                return bestResult;
                        }
                    }
                }
                return bestResult;
            }
            else
            {
                var bestResult = (score: Int32.MaxValue, depth: Int32.MaxValue);

                for (int y = 0; y < max_y; y++)
                {
                    for (int x = 0; x < max_x; x++)
                    {
                        if (_currentBoard.SetMove(x, y))
                        {
                            var result = MinMax(_currentBoard.Clone(), true, alpha, beta, depth + 1);
                            _currentBoard.UndoLastMove();

                            bestResult = Min(bestResult, result);

                            beta = Math.Min(beta, result.score);

                            /*if (result.score < bestResult.score) {
                                bestResult.score = result.score;
                                bestResult.depth = result.depth;
                            } else if (result.score == bestResult.score && result.depth < bestResult.depth) {
                                bestResult.depth = result.depth;
                            }*/

                            if (beta <= alpha)
                                return bestResult;
                        }
                    }
                }

                return bestResult;
            }
        }

        private static (int score, int depth) Max((int score, int depth) result1, (int score, int depth) result2)
        {
            var res = result1;
            if (result1.score > result2.score) 
            {
                res.score = result1.score;
                res.depth = result1.depth;
            }

            if (result2.score > result1.score)
            {
                res.score = result2.score;
                res.depth = result2.depth;
            }
            
            if (result1.score == result2.score)
            {
                res.depth = Math.Min(result1.depth, result2.depth);
            }

            return res;
            //return (score: Math.Max(result1.score, result2.depth), depth: Math.Min(result1.depth, result2.depth));
        }

        private static (int score, int depth) Min((int score, int depth) result1, (int score, int depth) result2)
        {
            var res = result1;
            if(result1.score < result2.score) {
                res.score = result1.score;
                res.depth = result1.depth;
            }

            if (result2.score < result1.score)
            {
                res.score = result2.score;
                res.depth = result2.depth;
            }
            
            if (result1.score == result2.score)
            {
                res.depth = Math.Min(result1.depth, result2.depth);
            }

            return res;
            //return (score: Math.Min(result1.score, result2.depth), depth: Math.Min(result1.depth, result2.depth));
        }
    }
}