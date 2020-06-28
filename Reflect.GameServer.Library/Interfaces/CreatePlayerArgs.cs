using Reflect.GameServer.Data.Models;

namespace Reflect.GameServer.Library.Interfaces
{
    public class CreatePlayerArgs
    {
        public IConnection Connection;

        public CreatePlayerArgs(BasePlayer player)
        {
            Connection = player.Connection;
            GameId = player.GameId;
            User = player.User;
        }

        public string GameId { get; set; }

        public UserInfo User { get; set; }
    }
}