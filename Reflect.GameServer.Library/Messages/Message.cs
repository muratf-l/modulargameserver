using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Reflect.GameServer.Library.Messages
{
    public class Message : IMessage
    {
        [JsonProperty("action")] public MessageAction Action { get; set; }

        [JsonProperty("data")] public JToken Data { get; set; }
    }
}