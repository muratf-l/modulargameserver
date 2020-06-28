using System;
using Reflect.GameServer.Data.Models;
using Reflect.GameServer.Library.Interfaces;
using Reflect.GameServer.Library.Messages;

namespace Reflect.GameServer.Library
{
    [Serializable]
    public class BasePlayer : MarshalByRefObject
    {
        internal BaseGame Game;

        public IConnection Connection { get; set; }

        public int Id { get; internal set; }

        public string GameId { get; set; } = string.Empty;

        public UserInfo User { get; set; }

        public PlayerStatus Status { get; set; }

        public void Disconnect()
        {
            Status = PlayerStatus.Offline;

            Game?.DoDisconnect(this);

            Connection.DoDisconnect();
        }

        public void Send(IMessage message)
        {
            Connection.Send(message);
        }
    }
}