using Newtonsoft.Json;

namespace Reflect.GameServer.Data.Models
{
    public enum PlayerStatus : byte
    {
        Offline = 0,
        Online = 10
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class PlayerInfo
    {
        [JsonProperty("index")] public int PlayerNo { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("picture")] public string Picture { get; set; }

        [JsonProperty("status")] public PlayerStatus OnlineStatus { get; set; }
    }
}