using System.Linq;
using Reflect.GameServer.Library.Helpers;
using Reflect.GameServer.Library.Messages;

namespace Reflect.Game.Ludo.Engine.Logic
{
    public class Board
    {
        private Square[] _squares;

        public GameCode Game;

        private int _currentTurn = -1;

        private int[] _currentDice;

        public void Build()
        {
            GenerateSquares();

            _currentTurn = RandomUtils.RollBy(0, Game.PlayerCount);

            TurnPlay();
        }

        public void Start()
        {
            var msg = new MessageGame
            {
                Action = MessageAction.GameData,
                GameId = Game.GameId,
                Body = new MessageGameBody
                {
                    Code = MessageGameAction.Started
                }
            };

            Game.Broadcast(msg);
        }

        #region "Turns"

        private void TurnPlay()
        {
            _currentDice = new int[3];

            _currentDice[0] = RandomUtils.Roll();

            if (_currentDice[0] == 6)
                _currentDice[1] = RandomUtils.Roll();

            repeat:
            if (_currentDice[1] == 6)
                _currentDice[2] = RandomUtils.Roll();

            if (_currentDice[2] == 6) goto repeat;

            _currentDice = _currentDice.Where(c => c > 0).ToArray();

            var msg = new MessageGame
            {
                Action = MessageAction.GameData,
                GameId = Game.GameId,
                Body = new MessageGameBody
                {
                    Code = MessageGameAction.Turn,
                    Data = new GameTurn
                    {
                        Dice = _currentDice,
                        PlayerNo = _currentTurn,
                        TimeOut = GameConst.PlayerTimeout
                    }
                }
            };

            Game.Broadcast(msg);
        }

        #endregion

        #region "Build"

        private void GenerateSquares()
        {
            _squares = new Square[GameConst.BoardSquareCount];

            for (var side = 0; side < GameConst.BoardSquareCount; side++) _squares[side] = new Square(side);

            foreach (var starSquare in GameConst.StarSquare) _squares[starSquare] = new SquareStar(starSquare);

            for (var playerId = 0; playerId < GameConst.PlayerSquare.Length; playerId++)
            {
                var positionId = GameConst.PlayerSquare[playerId];

                _squares[positionId] = new SquarePlayer(positionId) { PlayerNo = playerId };
            }
        }

        #endregion
    }
}