namespace Reflect.Game.Ludo.Engine.Logic
{
    public class SquarePlayer : Square
    {
        public int PlayerNo { get; set; }

        public SquarePlayer(int position) : base(position)
        {
            IsProtected = true;
        }
    }
}