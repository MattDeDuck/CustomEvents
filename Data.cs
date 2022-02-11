using System.Collections.Generic;
using Newtonsoft.Json;

namespace CustomEvent
{
    public class Data
    {
        public class EventObject
        {
            [JsonProperty("Room")]
            public string Room { get; set; }

            [JsonProperty("Sprite")]
            public string Sprite { get; set; }

            [JsonProperty("Rotation")]
            public float Rotation { get; set; }

            [JsonProperty("Location")]
            public List<float> Location { get; set; }

            [JsonProperty("XP")]
            public float XP { get; set; }

            [JsonProperty("Goal")]
            public List<string> Goal { get; set; }

            [JsonProperty("Sound")]
            public string Sound { get; set; }
        }
    }
}
