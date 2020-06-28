using System.Net;
using Reflect.GameServer.Library;
using Reflect.GameServer.Library.Interfaces;
using Reflect.GameServer.Library.Logging;
using Reflect.GameServer.Library.Messages;
using Reflect.GameServer.ServerCore;

namespace Reflect.GameServer.GameManager
{
    public interface IGameServer
    {
        public void TryStart();

        public void TryStop();
    }

    public class GameServer : WsServer, IGameServer
    {
        public GameServer(IPAddress address, int port) : base(address, port)
        {
        }

        public GameHost Host { get; set; }

        public void TryStart()
        {
            Start();
        }

        public void TryStop()
        {
            Stop();
        }

        protected override TcpSession CreateSession()
        {
            return new GameServerSocket(this) {Host = Host};
        }

        public class GameServerSocket : WsSession, IConnection
        {
            public GameServerSocket(WsServer server) : base(server)
            {
                Player = new BasePlayer {Connection = this, GameId = string.Empty};
            }

            public GameHost Host { get; set; }

            public BasePlayer Player { get; set; }

            public void Send(IMessage message)
            {
                var js = MessageUtil.Serialize(message);

                if (!string.IsNullOrEmpty(js))
                    SendTextAsync(js);
            }

            public void DoDisconnect()
            {
                Disconnect();
            }

            public override void OnWsConnected(HttpRequest request)
            {
                LogService.WriteDebug($@"WebSocket session with Id {Id} connected!");
                Host.RunEvent(GameEvent.SocketConnected, Player, null, null);
            }

            public override void OnWsDisconnected()
            {
                LogService.WriteDebug($@"WebSocket session with Id {Id} disconnected!");

                Host.RunEvent(GameEvent.SocketDisconnected, Player, null, null);
            }

            public override void OnWsReceived(byte[] buffer, long offset, long size)
            {
                var msg = MessageUtil.DeSerialize<Message>(buffer, offset, size);

                switch (msg.Action)
                {
                    case MessageAction.FacebookRegisterOrLogin:
                        Host.RunEvent(GameEvent.UserRegister, Player, msg, null);
                        break;

                    case MessageAction.MailRegister:
                        Host.RunEvent(GameEvent.UserRegister, Player, msg, null);
                        break;

                    case MessageAction.MailLogin:
                        Host.RunEvent(GameEvent.UserLogin, Player, msg, null);
                        break;

                    case MessageAction.GameJoin:
                        Host.RunEvent(GameEvent.GameJoin, Player, msg, null);
                        break;

                    case MessageAction.GameLeave:
                        Host.RunEvent(GameEvent.GameLeft, Player, msg, null);
                        break;

                    case MessageAction.GameData:
                        var gameMsg = MessageUtil.DeSerialize<MessageGame>(buffer, offset, size);

                        Host.RunEvent(GameEvent.GameData, Player, gameMsg, null);
                        break;
                }
            }
        }
    }
}