using System.Text.Json.Serialization;

namespace Saga.Models
{
    public class LoginResponse
    {
        [JsonPropertyName("user")]
        public User User { get; set; }

        [JsonPropertyName("userDefaultLibraryId")]
        public string UserDefaultLibraryId { get; set; }

        [JsonPropertyName("serverSettings")]
        public ServerSettings ServerSettings { get; set; }

        [JsonPropertyName("Source")]
        public string Source { get; set; }
    }
}
