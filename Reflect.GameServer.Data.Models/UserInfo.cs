using Newtonsoft.Json;

namespace Reflect.GameServer.Data.Models
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class UserInfo
    {
        [JsonProperty("token")] public string Id { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("coin")] public int Coin { get; set; }

        [JsonProperty("picture")] public string Picture { get; set; }
    }
}