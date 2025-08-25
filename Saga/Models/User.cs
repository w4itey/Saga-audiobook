using System.Text.Json.Serialization;

namespace Saga.Models
{
    public class User
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("mediaProgress")]
        public List<MediaProgress> MediaProgress { get; set; }

        [JsonPropertyName("seriesHideFromContinueListening")]
        public List<string> SeriesHideFromContinueListening { get; set; }

        [JsonPropertyName("bookmarks")]
        public List<AudioBookmark> Bookmarks { get; set; }

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }

        [JsonPropertyName("isLocked")]
        public bool IsLocked { get; set; }

        [JsonPropertyName("lastSeen")]
        public long? LastSeen { get; set; }

        [JsonPropertyName("createdAt")]
        public long CreatedAt { get; set; }

        [JsonPropertyName("permissions")]
        public UserPermissions Permissions { get; set; }

        [JsonPropertyName("librariesAccessible")]
        public List<string> LibrariesAccessible { get; set; }

        [JsonPropertyName("itemTagsAccessible")]
        public List<string> ItemTagsAccessible { get; set; }
    }

    public class UserPermissions
    {
        [JsonPropertyName("download")]
        public bool Download { get; set; }

        [JsonPropertyName("update")]
        public bool Update { get; set; }

        [JsonPropertyName("delete")]
        public bool Delete { get; set; }

        [JsonPropertyName("upload")]
        public bool Upload { get; set; }

        [JsonPropertyName("accessAllLibraries")]
        public bool AccessAllLibraries { get; set; }

        [JsonPropertyName("accessAllTags")]
        public bool AccessAllTags { get; set; }

        [JsonPropertyName("accessExplicitContent")]
        public bool AccessExplicitContent { get; set; }
    }

    public class MediaProgress
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("libraryItemId")]
        public string LibraryItemId { get; set; }

        [JsonPropertyName("episodeId")]
        public string EpisodeId { get; set; }

        [JsonPropertyName("duration")]
        public double Duration { get; set; }

        [JsonPropertyName("progress")]
        public double Progress { get; set; }

        [JsonPropertyName("currentTime")]
        public double CurrentTime { get; set; }

        [JsonPropertyName("isFinished")]
        public bool IsFinished { get; set; }

        [JsonPropertyName("hideFromContinueListening")]
        public bool HideFromContinueListening { get; set; }

        [JsonPropertyName("lastUpdate")]
        public long LastUpdate { get; set; }

        [JsonPropertyName("startedAt")]
        public long StartedAt { get; set; }

        [JsonPropertyName("finishedAt")]
        public long? FinishedAt { get; set; }
    }

    public class AudioBookmark
    {
        [JsonPropertyName("libraryItemId")]
        public string LibraryItemId { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("time")]
        public int Time { get; set; }

        [JsonPropertyName("createdAt")]
        public long CreatedAt { get; set; }
    }
}
