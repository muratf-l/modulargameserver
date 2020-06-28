using Newtonsoft.Json;

namespace Reflect.GameServer.Library.Messages
{
    public class MessageGameBody
    {
        [JsonProperty("code")] public MessageGameAction Code { get; set; }

        [JsonProperty("data")] public object Data { get; set; }
    }
}