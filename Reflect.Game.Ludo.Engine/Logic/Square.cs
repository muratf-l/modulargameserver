using System.Collections.Generic;

namespace Reflect.Game.Ludo.Engine.Logic
{
    public class Square
    {
        public Square(int position)
        {
            Position = position;

            Pieces = new List<Piece>();
        }

        public int Position { get; protected set; }

        public List<Piece> Pieces { get; protected set; }

        public bool IsProtected { get; protected set; }

        public void AddPiece(Piece piece)
        {
            Pieces.Add(piece);
        }

        public void RemovePiece(Piece piece)
        {
            Pieces.Remove(piece);
        }
    }
}