using Newtonsoft.Json;

namespace Rewrite.Utilities
{
    [JsonObject]
    public class EncodeConfig
    {

        public int DitheringMode { get; set; }
        public int ColorMode { get; set; }

        public bool Accurate { get; set; }
        public int Contrast { get; set; }

        public string InputFolder { get; set; }
        public string InputFilename { get; set; }

        public bool Split { get; set; }
        public int SplitAmount { get; set; }
        public bool DeleteOnFinish { get; set; }

        public EncodeConfig() { }
    }
}
