using System.Text.Json.Serialization;

namespace Saga.Models
{
    public class ServerInfo
    {
        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("buildNumber")]
        public string BuildNumber { get; set; }

        [JsonPropertyName("isInit")]
        public bool IsInitialized { get; set; }

        [JsonPropertyName("authMethods")]
        public AuthMethod[] AuthMethods { get; set; } = new AuthMethod[0];

        [JsonPropertyName("language")]
        public string Language { get; set; }

        [JsonPropertyName("serverName")]
        public string ServerName { get; set; }

        public bool HasLocalAuth => AuthMethods?.Any(m => m.Type == "local") == true;
        public bool HasOpenIdAuth => AuthMethods?.Any(m => m.Type == "openid") == true;
        public AuthMethod[] OpenIdProviders => AuthMethods?.Where(m => m.Type == "openid").ToArray() ?? new AuthMethod[0];
    }

    public class AuthMethod
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } // "local" or "openid"

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("provider")]
        public string Provider { get; set; } // For OpenID: "google", "microsoft", etc.

        [JsonPropertyName("issuer")]
        public string Issuer { get; set; }

        [JsonPropertyName("buttonColor")]
        public string ButtonColor { get; set; }

        [JsonPropertyName("textColor")]
        public string TextColor { get; set; }
    }
}