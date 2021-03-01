using Newtonsoft.Json;

namespace EncodeAndSign
{
    [JsonObject]
    public class Config
    {
        public int DitheringMode { get; set; }
        public bool Accurate { get; set; }
        public bool Split { get; set; }

        public Config() { }
    }
}
