using System.Text.Json.Serialization;

namespace Saga.Models
{
    public class ServerSettings
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("scannerFindCovers")]
        public bool ScannerFindCovers { get; set; }

        [JsonPropertyName("scannerCoverProvider")]
        public string ScannerCoverProvider { get; set; }

        [JsonPropertyName("storeCoverWithItem")]
        public bool StoreCoverWithItem { get; set; }

        [JsonPropertyName("storeMetadataWithItem")]
        public bool StoreMetadataWithItem { get; set; }

        [JsonPropertyName("metadataFileFormat")]
        public string MetadataFileFormat { get; set; }
    }
}
