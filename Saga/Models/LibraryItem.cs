using System.Text.Json.Serialization;

namespace Saga.Models
{
    public class LibraryItem
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("libraryId")]
        public string LibraryId { get; set; }

        [JsonPropertyName("folderId")]
        public string FolderId { get; set; }

        [JsonPropertyName("path")]
        public string Path { get; set; }

        [JsonPropertyName("relPath")]
        public string RelPath { get; set; }

        [JsonPropertyName("isFile")]
        public bool IsFile { get; set; }

        [JsonPropertyName("mtimeMs")]
        public long MtimeMs { get; set; }

        [JsonPropertyName("ctimeMs")]
        public long CtimeMs { get; set; }

        [JsonPropertyName("birthtimeMs")]
        public long BirthtimeMs { get; set; }

        [JsonPropertyName("addedAt")]
        public long AddedAt { get; set; }

        [JsonPropertyName("updatedAt")]
        public long UpdatedAt { get; set; }

        [JsonPropertyName("lastScan")]
        public long? LastScan { get; set; }

        [JsonPropertyName("scanVersion")]
        public string ScanVersion { get; set; }

        [JsonPropertyName("isMissing")]
        public bool IsMissing { get; set; }

        [JsonPropertyName("isInvalid")]
        public bool IsInvalid { get; set; }

        [JsonPropertyName("mediaType")]
        public string MediaType { get; set; }

        [JsonPropertyName("media")]
        public Media Media { get; set; }

        [JsonPropertyName("libraryFiles")]
        public List<LibraryFile> LibraryFiles { get; set; }
    }

    public class Media
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("metadata")]
        public Metadata Metadata { get; set; }

        [JsonPropertyName("coverPath")]
        public string CoverPath { get; set; }

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; }

        [JsonPropertyName("audioFiles")]
        public List<AudioFile> AudioFiles { get; set; }

        [JsonPropertyName("chapters")]
        public List<Chapter> Chapters { get; set; }

        [JsonPropertyName("duration")]
        public double Duration { get; set; }

        [JsonPropertyName("size")]
        public long Size { get; set; }

        [JsonPropertyName("tracks")]
        public List<AudioTrack> Tracks { get; set; }
    }

    public class Metadata
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("author")]
        public string Author { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("releaseDate")]
        public string ReleaseDate { get; set; }

        [JsonPropertyName("genres")]
        public List<string> Genres { get; set; }

        [JsonPropertyName("publishedYear")]
        public string PublishedYear { get; set; }

        [JsonPropertyName("publishedDate")]
        public string PublishedDate { get; set; }

        [JsonPropertyName("publisher")]
        public string Publisher { get; set; }

        [JsonPropertyName("language")]
        public string Language { get; set; }

        [JsonPropertyName("isbn")]
        public string Isbn { get; set; }

        [JsonPropertyName("asin")]
        public string Asin { get; set; }

        [JsonPropertyName("explicit")]
        public bool Explicit { get; set; }

        [JsonPropertyName("authorName")]
        public string AuthorName { get; set; }

        [JsonPropertyName("narratorName")]
        public string NarratorName { get; set; }

        [JsonPropertyName("seriesName")]
        public string SeriesName { get; set; }
    }

    public class LibraryFile
    {
        [JsonPropertyName("ino")]
        public string Ino { get; set; }

        [JsonPropertyName("metadata")]
        public FileMetadata Metadata { get; set; }

        [JsonPropertyName("addedAt")]
        public long AddedAt { get; set; }

        [JsonPropertyName("updatedAt")]
        public long UpdatedAt { get; set; }

        [JsonPropertyName("fileType")]
        public string FileType { get; set; }
    }

    public class FileMetadata
    {
        [JsonPropertyName("filename")]
        public string Filename { get; set; }

        [JsonPropertyName("ext")]
        public string Ext { get; set; }

        [JsonPropertyName("path")]
        public string Path { get; set; }

        [JsonPropertyName("relPath")]
        public string RelPath { get; set; }

        [JsonPropertyName("size")]
        public long Size { get; set; }

        [JsonPropertyName("mtimeMs")]
        public long MtimeMs { get; set; }

        [JsonPropertyName("ctimeMs")]
        public long CtimeMs { get; set; }

        [JsonPropertyName("birthtimeMs")]
        public long BirthtimeMs { get; set; }
    }

    public class AudioFile
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("ino")]
        public string Ino { get; set; }

        [JsonPropertyName("metadata")]
        public FileMetadata Metadata { get; set; }

        [JsonPropertyName("addedAt")]
        public long AddedAt { get; set; }

        [JsonPropertyName("updatedAt")]
        public long UpdatedAt { get; set; }

        [JsonPropertyName("trackNumFromMeta")]
        public int? TrackNumFromMeta { get; set; }

        [JsonPropertyName("discNumFromMeta")]
        public int? DiscNumFromMeta { get; set; }

        [JsonPropertyName("trackNumFromFilename")]
        public int? TrackNumFromFilename { get; set; }

        [JsonPropertyName("discNumFromFilename")]
        public int? DiscNumFromFilename { get; set; }

        [JsonPropertyName("manuallyVerified")]
        public bool ManuallyVerified { get; set; }

        [JsonPropertyName("invalid")]
        public bool Invalid { get; set; }

        [JsonPropertyName("exclude")]
        public bool Exclude { get; set; }

        [JsonPropertyName("error")]
        public string Error { get; set; }

        [JsonPropertyName("format")]
        public string Format { get; set; }

        [JsonPropertyName("duration")]
        public double Duration { get; set; }

        [JsonPropertyName("bitRate")]
        public int BitRate { get; set; }

        [JsonPropertyName("language")]
        public string Language { get; set; }

        [JsonPropertyName("codec")]
        public string Codec { get; set; }

        [JsonPropertyName("timeBase")]
        public string TimeBase { get; set; }

        [JsonPropertyName("channels")]
        public int Channels { get; set; }

        [JsonPropertyName("channelLayout")]
        public string ChannelLayout { get; set; }

        [JsonPropertyName("chapters")]
        public List<Chapter> Chapters { get; set; }

        [JsonPropertyName("embeddedCoverArt")]
        public string EmbeddedCoverArt { get; set; }

        [JsonPropertyName("metaTags")]
        public MetaTags MetaTags { get; set; }

        [JsonPropertyName("mimeType")]
        public string MimeType { get; set; }
    }

    public class Chapter
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("start")]
        public double Start { get; set; }

        [JsonPropertyName("end")]
        public double End { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }
    }

    public class AudioTrack
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("startOffset")]
        public double StartOffset { get; set; }

        [JsonPropertyName("duration")]
        public double Duration { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("contentUrl")]
        public string ContentUrl { get; set; }

        [JsonPropertyName("mimeType")]
        public string MimeType { get; set; }

        [JsonPropertyName("metadata")]
        public FileMetadata Metadata { get; set; }
    }

    public class MetaTags
    {
        [JsonPropertyName("tagAlbum")]
        public string TagAlbum { get; set; }

        [JsonPropertyName("tagArtist")]
        public string TagArtist { get; set; }

        [JsonPropertyName("tagGenre")]
        public string TagGenre { get; set; }

        [JsonPropertyName("tagTitle")]
        public string TagTitle { get; set; }

        [JsonPropertyName("tagTrack")]
        public string TagTrack { get; set; }

        [JsonPropertyName("tagAlbumArtist")]
        public string TagAlbumArtist { get; set; }

        [JsonPropertyName("tagComposer")]
        public string TagComposer { get; set; }

        [JsonPropertyName("tagDate")]
        public string TagDate { get; set; }
    }

    public class LibraryItemsResponse
    {
        [JsonPropertyName("results")]
        public List<LibraryItem> Results { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("limit")]
        public int Limit { get; set; }

        [JsonPropertyName("page")]
        public int Page { get; set; }

        [JsonPropertyName("sortBy")]
        public string SortBy { get; set; }

        [JsonPropertyName("sortDesc")]
        public bool SortDesc { get; set; }

        [JsonPropertyName("filterBy")]
        public string FilterBy { get; set; }

        [JsonPropertyName("mediaType")]
        public string MediaType { get; set; }

        [JsonPropertyName("minified")]
        public bool Minified { get; set; }

        [JsonPropertyName("collapseSeries")]
        public bool CollapseSeries { get; set; }
    }
}