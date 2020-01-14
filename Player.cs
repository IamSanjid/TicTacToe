using System;
using System.Collections.Generic;

namespace TicTacToe
{
    public enum PlayerType
    {
        Type_O = 'o',
        Type_X = 'x'
    }

    public class Player : ICloneable
    {
        public int X = 0;
        public int Y = 0;

        public PlayerType Type { get; private set; }

        public List<(int x, int y)> Moves;

        public Player(PlayerType pt)
        {
            Type = pt;
            Moves = new List<(int x, int y)>();
        }

        public void SetMove()
        {
            Moves.Add((x: X, y: Y));
        }

        public void RemoveLastMove()
        {
            if (Moves.Count > 0)
                Moves.Remove(Moves[Moves.Count - 1]);
        }

        public bool IsWinner(Board board)
        {            
            return false;
        }

        object ICloneable.Clone()
        {
            var player = new Player(this.Type){ X = this.X, Y = this.Y };
            foreach(var move in this.Moves)
            {
                player.Moves.Add((x: move.x, y: move.y));
            }
            return player;
        }
    }
}