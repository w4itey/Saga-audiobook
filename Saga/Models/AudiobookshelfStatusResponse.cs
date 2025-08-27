using System.Text.Json.Serialization;

namespace Saga.Models
{
    public class AudiobookshelfStatusResponse
    {
        [JsonPropertyName("isInit")]
        public bool IsInit { get; set; }

        [JsonPropertyName("language")]
        public string Language { get; set; }

        [JsonPropertyName("ConfigPath")]
        public string ConfigPath { get; set; }

        [JsonPropertyName("MetadataPath")]
        public string MetadataPath { get; set; }
    }
}