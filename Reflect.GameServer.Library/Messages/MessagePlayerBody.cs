using System.Net;
using Newtonsoft.Json;

namespace Reflect.GameServer.Library.Messages
{
    public class MessagePlayerBody
    {
        [JsonProperty("code")] public HttpStatusCode Code { get; set; }

        [JsonProperty("data")] public object Data { get; set; }
    }
}