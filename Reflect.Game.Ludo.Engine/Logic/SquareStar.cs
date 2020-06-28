namespace Reflect.Game.Ludo.Engine.Logic
{
    public class SquareStar : Square
    {
        public SquareStar(int position) : base(position)
        {
            IsProtected = true;
        }
    }
}