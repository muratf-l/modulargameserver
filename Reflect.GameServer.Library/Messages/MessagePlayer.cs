using Newtonsoft.Json;

namespace Reflect.GameServer.Library.Messages
{
    public class MessagePlayer : IMessage
    {
        [JsonProperty("action")] public MessageAction Action { get; set; }

        [JsonProperty("body")] public MessagePlayerBody Body { get; set; }
    }
}