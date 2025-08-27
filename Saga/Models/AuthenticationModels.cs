using System.Text.Json.Serialization;

namespace Saga.Models
{
    public class AuthenticationResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public UserProfile User { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public AuthenticationProvider Provider { get; set; }
    }

    public class UserProfile
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string AvatarUrl { get; set; }
        public AuthenticationProvider Provider { get; set; }
        public string ProviderId { get; set; }
        public List<string> ServerUrls { get; set; } = new List<string>();
        public UserPreferences Preferences { get; set; } = new UserPreferences();
        public DateTime CreatedAt { get; set; }
        public DateTime LastLoginAt { get; set; }
    }

    public class UserPreferences
    {
        public double PlaybackSpeed { get; set; } = 1.0;
        public int SleepTimerMinutes { get; set; } = 30;
        public bool AutoDownload { get; set; } = false;
        public bool ShareReadingProgress { get; set; } = true;
        public ThemeMode Theme { get; set; } = ThemeMode.System;
        public Dictionary<string, object> CustomSettings { get; set; } = new Dictionary<string, object>();
    }

    public enum AuthenticationProvider
    {
        Audiobookshelf,
        Google,
        Apple,
        Microsoft,
        Facebook,
        GitHub
    }

    public enum ThemeMode
    {
        Light,
        Dark,
        System
    }

    public class SSOConfiguration
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string RedirectUri { get; set; }
        public List<string> Scopes { get; set; } = new List<string>();
        public string Authority { get; set; }
    }

    public class MultiUserSession
    {
        public string PrimaryUserId { get; set; }
        public List<UserProfile> AvailableUsers { get; set; } = new List<UserProfile>();
        public UserProfile CurrentUser { get; set; }
        public Dictionary<string, string> UserTokens { get; set; } = new Dictionary<string, string>();
    }

    // OAuth-specific models
    public class OAuthTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; }
    }

    public class SocialUserInfo
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("picture")]
        public string Picture { get; set; }

        [JsonPropertyName("given_name")]
        public string GivenName { get; set; }

        [JsonPropertyName("family_name")]
        public string FamilyName { get; set; }
    }
}