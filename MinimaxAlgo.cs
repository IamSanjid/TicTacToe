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

        private static Moves GetMoves(Board _currentBoard, int depth)
        {
            //int bestScore = int.MinValue;
            //int minDepth = int.MaxValue;
            var bestResult = (score: Int32.MinValue, depth: Int32.MaxValue);

            var best_moves = new Dictionary<(int x, int y), (int score, int depth)>();
            var worse_moves = new List<(int x, int y)>();

            for (int y = 0, max_y = _currentBoard.GetMoves().GetLength(0); y < max_y; y++)
            {
                for (int x = 0, max_x = _currentBoard.GetMoves().GetLength(1); x < max_x; x++)
                {
                    if (_currentBoard.GetMoves()[y, x] == ' ')
                    {
                        _currentBoard.SetMove(x, y, (char)_currentBoard.CurrentPlayer.Type);

                        var result = MinMax(_currentBoard.Clone(), false, int.MinValue, int.MaxValue, depth);
                        if ((result.score >= bestResult.score)
                            || (result.score == bestResult.score && result.depth < bestResult.depth))
                        {
                            best_moves.Add((x, y), result);
                            bestResult = result;
                        }
                        else
                            worse_moves.Add((x, y));

                        _currentBoard.UndoLastMove();
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
                if (mv.Value.score != moves.BestResult.score || mv.Value.depth != moves.BestResult.depth)
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
                
                if (res == MatchResult.Tie) result.score = 0;
                if (isMaximizingPlayer && 
                    res.ToString().ToLower()[0] == (char)_currentBoard.CurrentPlayer.Type)
                {
                    result.score = 10;
                }
                else if (!isMaximizingPlayer && res.ToString().ToLower()[0] != (char)_currentBoard.CurrentPlayer.Type)
                    result.score = 10;
                
                return result;
            }

            if (isMaximizingPlayer)
            {
                var bestResult = (score: Int32.MinValue, depth: Int32.MaxValue);
                //int bestScore = Int32.MinValue;
                //int bestDepth = Int32.MaxValue;

                for (int y = 0, max_y = _currentBoard.GetMoves().GetLength(0); y < max_y; y++)
                {
                    for (int x = 0, max_x = _currentBoard.GetMoves().GetLength(1); x < max_x; x++)
                    {
                        if (_currentBoard.GetMoves()[y, x] == ' ')
                        {
                            _currentBoard.SetMove(x, y, (char)_currentBoard.CurrentPlayer.Type);
                            
                            var result = MinMax(_currentBoard.Clone(), false, alpha, beta, depth + 1);
                            bestResult = Max(result, bestResult);
                            //int score = result.score;
                            alpha = Math.Max(alpha, result.score);

                            /*if(score > bestScore) {
                                bestScore = score;
                                bestDepth = result.depth;
                            } else if (score == bestScore && result.depth < bestDepth) {
                                bestDepth = result.depth;
                            }*/
                            
                            if (beta <= alpha)
                                break;
                            
                            _currentBoard.UndoLastMove();
                        }
                    }
                }
                return bestResult;
            }
            else
            {
                var bestResult = (score: Int32.MaxValue, depth: Int32.MaxValue);
                //int bestScore = Int32.MaxValue;
                //int bestDepth = Int32.MaxValue;

                for (int y = 0, max_y = _currentBoard.GetMoves().GetLength(0); y < max_y; y++)
                {
                    for (int x = 0, max_x = _currentBoard.GetMoves().GetLength(1); x < max_x; x++)
                    {
                        if (_currentBoard.GetMoves()[y, x] == ' ')
                        {
                            _currentBoard.SetMove(x, y, (char)_currentBoard.CurrentPlayer.Type);
                            
                            var result = MinMax(_currentBoard.Clone(), true, alpha, beta, depth + 1);
                            bestResult = Min(result, bestResult);
                            //int score = result.score;
                            //bestScore = Math.Min(bestScore, score);
                            beta = Math.Min(beta, result.score);

                            /*if(score < bestScore) {
                                bestScore = score;
                                bestDepth = result.depth;
                            } else if (score == bestScore && result.depth < bestDepth) {
                                bestDepth = result.depth;
                            }*/

                            if (beta <= alpha)
                                break;

                            _currentBoard.UndoLastMove();
                        }
                    }
                }
                return bestResult;
            }
        }

        private static (int score, int depth) Max((int score, int depth) result1, (int score, int depth) result2)
        {
            var res = result1;
            if (result1.score > result2.score) {
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
                res.depth = result1.depth < result2.depth ? result2.depth : result1.depth;
            }

            return res;
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
                res.depth = result1.depth < result2.depth ? result2.depth : result1.depth;
            }

            return res;
        }
    }
}