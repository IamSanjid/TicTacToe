using System;
using System.Collections.Generic;
using System.Linq;

namespace TicTacToe
{
    public enum Kind
    {
        O = 'o',
        X = 'x',
        Tie = 't',
        None = 'n'
    }

    public class Player : ICloneable
    {
        static double Map(double a1, double a2, double b1, double b2, double s) => b1 + (s - a1) * (b2 - b1) / (a2 - a1);

        public int X = 0;
        public int Y = 0;

        public Kind Type { get; private set; }

        public int TotalMoves;
        
        private byte[] set_vals = new byte[3*3];
        //int score = 0;

        public Player(Kind pt)
        {
            Type = pt;
            TotalMoves = 0;
        }

        public void SetMove()
        {
            set_vals[Y * 3 + X] = 1;
            TotalMoves++;
        }

        public void RemoveMove(int x, int y)
        {
            set_vals[y * 3 + x] = 0;
            TotalMoves--;
        }

        public bool IsWinner(Board board)
        {
            if (TotalMoves < board.MatchDepth) return false;

            var res = false;
            int mathced = 0;

            for (int i = 0; i < set_vals.Length; i++)
            {
                
            }
            return res;
        }

        object ICloneable.Clone()
        {
            var player = new Player(this.Type){ X = this.X, Y = this.Y, set_vals = new byte[set_vals.Length], TotalMoves = this.TotalMoves };
            Buffer.BlockCopy(this.set_vals, 0, player.set_vals, 0, this.set_vals.Length * sizeof(byte));
            return player;
        }
    }
}