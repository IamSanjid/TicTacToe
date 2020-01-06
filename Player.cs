namespace TicTacToe
{
    public enum PlayerType
    {
        Type_O = 'o',
        Type_X = 'x'
    }

    public class Player
    {
        public int X = 0;
        public int Y = 0;

        public PlayerType Type { get; private set; }

        public Player(PlayerType pt)
        {
            Type = pt;
        }

        public Player Clone()
        {
            return new Player(this.Type){ X = this.X, Y = this.Y };
        }
    }
}