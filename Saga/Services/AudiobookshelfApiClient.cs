using Saga.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Saga.Services
{
    public class AudiobookshelfApiClient
    {
        private readonly HttpClient _httpClient;

        public AudiobookshelfApiClient()
        {
            _httpClient = new HttpClient();
        }

        public async Task<LoginResponse> LoginAsync(string serverUrl, string username, string password)
        {
            var loginRequest = new LoginRequest
            {
                Username = username,
                Password = password
            };

            var json = JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{serverUrl}/login", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<LoginResponse>(responseContent);
            }

            return null;
        }

        // OAuth2/SSO Authentication using Audiobookshelf's built-in SSO
        public async Task<string> StartSSOAuthAsync(string serverUrl, string redirectUri)
        {
            try
            {
                // Generate PKCE parameters
                var codeVerifier = GenerateCodeVerifier();
                var codeChallenge = GenerateCodeChallenge(codeVerifier);
                var state = Guid.NewGuid().ToString();

                // Store code verifier for later use
                await SecureStorage.SetAsync("oauth_code_verifier", codeVerifier);
                await SecureStorage.SetAsync("oauth_state", state);

                // Build OAuth authorization URL
                var authUrl = $"{serverUrl}/auth/openid?" +
                    $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
                    $"state={state}&" +
                    $"code_challenge={codeChallenge}&" +
                    $"code_challenge_method=S256";

                return authUrl;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StartSSOAuthAsync Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<LoginResponse> CompleteSSOAuthAsync(string serverUrl, string authorizationCode, string state)
        {
            try
            {
                // Verify state parameter
                var storedState = await SecureStorage.GetAsync("oauth_state");
                if (storedState != state)
                {
                    throw new InvalidOperationException("Invalid state parameter");
                }

                // Get stored code verifier
                var codeVerifier = await SecureStorage.GetAsync("oauth_code_verifier");
                
                // Exchange authorization code for tokens
                var response = await _httpClient.PostAsync($"{serverUrl}/auth/openid/callback", 
                    new StringContent(JsonSerializer.Serialize(new {
                        code = authorizationCode,
                        code_verifier = codeVerifier
                    }), Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent);
                    
                    // Clean up stored OAuth data
                    SecureStorage.Remove("oauth_code_verifier");
                    SecureStorage.Remove("oauth_state");
                    
                    return loginResponse;
                }
                
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CompleteSSOAuthAsync Exception: {ex.Message}");
                throw;
            }
        }

        private string GenerateCodeVerifier()
        {
            var bytes = new byte[32];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private string GenerateCodeChallenge(string codeVerifier)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
                return Convert.ToBase64String(challengeBytes)
                    .TrimEnd('=')
                    .Replace('+', '-')
                    .Replace('/', '_');
            }
        }

        public async Task<List<Library>> GetLibrariesAsync(string serverUrl, string token)
        {
            try
            {
                // Set timeout to prevent infinite loading
                _httpClient.Timeout = TimeSpan.FromSeconds(30);
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                
                var url = $"{serverUrl}/api/libraries";
                System.Diagnostics.Debug.WriteLine($"Making request to: {url}");
                
                var response = await _httpClient.GetAsync(url);
                
                System.Diagnostics.Debug.WriteLine($"Response status: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Response content: {responseContent}");
                    
                    var librariesResponse = JsonSerializer.Deserialize<LibrariesResponse>(responseContent, new JsonSerializerOptions 
                    { 
                        PropertyNameCaseInsensitive = true 
                    });
                    
                    System.Diagnostics.Debug.WriteLine($"Deserialized libraries: {librariesResponse?.Libraries?.Count ?? 0}");
                    return librariesResponse?.Libraries;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"HTTP Error: {response.StatusCode} - {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetLibrariesAsync Exception: {ex.Message}");
                throw;
            }

            return null;
        }

        public async Task<List<LibraryItem>> GetLibraryItemsAsync(string serverUrl, string token, string libraryId)
        {
            try
            {
                // Set timeout to prevent infinite loading
                _httpClient.Timeout = TimeSpan.FromSeconds(30);
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                
                var url = $"{serverUrl}/api/libraries/{libraryId}/items";
                System.Diagnostics.Debug.WriteLine($"Making request to: {url}");
                
                var response = await _httpClient.GetAsync(url);
                
                System.Diagnostics.Debug.WriteLine($"Response status: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Response content: {responseContent}");
                    
                    var itemsResponse = JsonSerializer.Deserialize<LibraryItemsResponse>(responseContent, new JsonSerializerOptions 
                    { 
                        PropertyNameCaseInsensitive = true 
                    });
                    
                    System.Diagnostics.Debug.WriteLine($"Deserialized library items: {itemsResponse?.Results?.Count ?? 0}");
                    return itemsResponse?.Results;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"HTTP Error: {response.StatusCode} - {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetLibraryItemsAsync Exception: {ex.Message}");
                throw;
            }

            return null;
        }
    }
}
