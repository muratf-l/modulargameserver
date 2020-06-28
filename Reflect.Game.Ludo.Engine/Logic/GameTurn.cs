using Newtonsoft.Json;

namespace Reflect.Game.Ludo.Engine.Logic
{
    public class GameTurn
    {
        [JsonProperty("index")] public int PlayerNo { get; set; }

        [JsonProperty("dice")] public int[] Dice { get; set; }

        [JsonProperty("timeout")] public int TimeOut { get; set; }
    }
}