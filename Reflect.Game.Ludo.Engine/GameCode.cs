using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json.Linq;
using Reflect.Game.Ludo.Engine.Logic;
using Reflect.GameServer.Data.Models;
using Reflect.GameServer.Library;
using Reflect.GameServer.Library.Messages;

namespace Reflect.Game.Ludo.Engine
{
    public class GameCode : Game<GamePlayer>
    {
        private Board _board;

        public override void GameInit()
        {
            _board = new Board { Game = this };
        }

        public override void GameStarted()
        {
            _board.Build();

            for (var i = 0; i < PlayerCount; i++)
                GetPlayer(i).Build();

            _board.Start();
        }

        public override void GameClosed()
        {

        }

        public override bool PlayerAllowed(GamePlayer player)
        {
            if (IsFull()) return false;

            return true;
        }

        public override void PlayerJoined(GamePlayer player)
        {
            player.PlayerNo = PlayerCount - 1;

            player.Send(new MessagePlayer
            {
                Action = MessageAction.GameJoin,
                Body = new MessagePlayerBody { Code = HttpStatusCode.OK, Data = GameId }
            });

            SendPlayerList();
        }

        public override void PlayerLeft(GamePlayer player)
        {
            SendPlayerList();
        }

        public override void PlayerMessage(GamePlayer player, MessageGame message)
        {
 
        }

        private void SendPlayerList()
        {
            var list = new List<PlayerInfo>();

            for (var i = 0; i < PlayerCount; i++)
            {
                var gamePlayer = GetPlayer(i);

                var pl = new PlayerInfo
                {
                    Name = gamePlayer.User.Name,
                    Picture = gamePlayer.User.Picture,
                    PlayerNo = gamePlayer.PlayerNo,
                    OnlineStatus = gamePlayer.Status
                };

                list.Add(pl);
            }

            var msg = new MessageGame
            {
                Action = MessageAction.GameData,
                GameId = GameId,
                Body = new MessageGameBody
                {
                    Code = MessageGameAction.PlayerList,
                    Data = JToken.FromObject(list)
                }
            };

            Broadcast(msg);
        }
    }
}