using Newtonsoft.Json;

namespace Reflect.GameServer.Library.Messages
{
    public class MessageGame : IMessage
    {
        [JsonProperty("action")] public MessageAction Action { get; set; }

        [JsonProperty("body")] public MessageGameBody Body { get; set; }

        [JsonProperty("game")] public string GameId { get; set; }
    }
}