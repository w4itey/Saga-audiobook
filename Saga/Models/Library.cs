using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Saga.Models
{
    public class Library
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("folders")]
        public List<Folder> Folders { get; set; }

        [JsonPropertyName("displayOrder")]
        public int DisplayOrder { get; set; }

        [JsonPropertyName("icon")]
        public string Icon { get; set; }

        [JsonPropertyName("mediaType")]
        public string MediaType { get; set; }

        [JsonPropertyName("provider")]
        public string Provider { get; set; }

        [JsonPropertyName("settings")]
        public LibrarySettings Settings { get; set; }

        [JsonPropertyName("createdAt")]
        public long CreatedAt { get; set; }

        [JsonPropertyName("lastUpdate")]
        public long LastUpdate { get; set; }
    }

    public class Folder
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("fullPath")]
        public string FullPath { get; set; }

        [JsonPropertyName("libraryId")]
        public string LibraryId { get; set; }

        [JsonPropertyName("addedAt")]
        public long AddedAt { get; set; }
    }

    public class LibrarySettings
    {
        [JsonPropertyName("coverAspectRatio")]
        public int CoverAspectRatio { get; set; }

        [JsonPropertyName("disableWatcher")]
        public bool DisableWatcher { get; set; }

        [JsonPropertyName("skipMatchingMediaWithAsin")]
        public bool SkipMatchingMediaWithAsin { get; set; }

        [JsonPropertyName("skipMatchingMediaWithIsbn")]
        public bool SkipMatchingMediaWithIsbn { get; set; }

        [JsonPropertyName("autoScanCronExpression")]
        public string AutoScanCronExpression { get; set; }
    }

    public class LibrariesResponse
    {
        [JsonPropertyName("libraries")]
        public List<Library> Libraries { get; set; }
    }
}
