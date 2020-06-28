using Reflect.GameServer.Library.Messages;

namespace Reflect.GameServer.Library.Interfaces
{
    public interface IConnection
    {
        void Send(IMessage message);

        void DoDisconnect();
    }
}