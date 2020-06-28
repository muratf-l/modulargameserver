using Reflect.Game.Ludo.Engine.Logic;
using Reflect.GameServer.Library;

namespace Reflect.Game.Ludo.Engine
{
    public class GamePlayer : BasePlayer
    {
        public int PlayerNo { get; set; }

        public int PositionStart { get; set; }

        public Piece[] Pieces { get; set; }

        public void Build()
        {
            PositionStart = GameConst.PlayerSquare[PlayerNo];

            Pieces = new Piece[GameConst.PlayerPiecesPerPlayer];

            for (var pIndex = 0; pIndex < GameConst.PlayerPiecesPerPlayer; pIndex++)
                Pieces[pIndex] = new Piece(PlayerNo, pIndex, PositionStart, GameConst.PlayerFinishSquareCount);
        }
    }
}