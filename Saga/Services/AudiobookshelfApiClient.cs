using Saga.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

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

        public async Task<ServerInfo> GetServerInfoAsync(string serverUrl)
        {
            try
            {
                // Set timeout for server discovery
                _httpClient.Timeout = TimeSpan.FromSeconds(10);
                
                System.Diagnostics.Debug.WriteLine($"Attempting to connect to: {serverUrl.TrimEnd('/')}/status");
                
                // First try a simple ping to test basic connectivity
                try
                {
                    var pingResponse = await _httpClient.GetAsync($"{serverUrl.TrimEnd('/')}/ping");
                    System.Diagnostics.Debug.WriteLine($"Ping response: {pingResponse.StatusCode}");
                }
                catch (Exception pingEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Ping failed: {pingEx.Message}");
                }
                
                // Use the correct Audiobookshelf status endpoint
                var response = await _httpClient.GetAsync($"{serverUrl.TrimEnd('/')}/status");

                System.Diagnostics.Debug.WriteLine($"Status response code: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Server status response: {responseContent}");
                    
                    // Parse the basic server status response
                    var statusResponse = JsonSerializer.Deserialize<AudiobookshelfStatusResponse>(responseContent, new JsonSerializerOptions 
                    { 
                        PropertyNameCaseInsensitive = true 
                    });

                    if (statusResponse != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Server initialized: {statusResponse.IsInit}, Language: {statusResponse.Language}");
                        
                        // Create ServerInfo with both local auth and SSO options
                        // Since Audiobookshelf doesn't provide an endpoint to discover SSO providers,
                        // we'll assume both local and SSO are available if server is initialized
                        var serverInfo = new ServerInfo
                        {
                            Version = "Unknown", // Status endpoint doesn't provide version
                            IsInitialized = statusResponse.IsInit,
                            Language = statusResponse.Language ?? "en-us",
                            ServerName = "Audiobookshelf", // Default name
                            AuthMethods = statusResponse.IsInit ? new AuthMethod[]
                            {
                                new AuthMethod { Type = "local", Name = "Username & Password" },
                                new AuthMethod { Type = "openid", Name = "Single Sign-On", Id = "sso" }
                            } : new AuthMethod[]
                            {
                                new AuthMethod { Type = "local", Name = "Username & Password" }
                            }
                        };

                        return serverInfo;
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Server status request failed: {response.StatusCode} - {response.ReasonPhrase}");
                    System.Diagnostics.Debug.WriteLine($"Error response: {errorContent}");
                }
            }
            catch (HttpRequestException httpEx)
            {
                System.Diagnostics.Debug.WriteLine($"HTTP Request Exception: {httpEx.Message}");
            }
            catch (TaskCanceledException timeoutEx)
            {
                System.Diagnostics.Debug.WriteLine($"Request timed out: {timeoutEx.Message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetServerInfoAsync Exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Exception type: {ex.GetType().Name}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
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

        public async Task<LoginResponse> ExchangeCodeForTokenAsync(string serverUrl, string code, string redirectUri, string codeVerifier)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== Token Exchange Started ===");
                System.Diagnostics.Debug.WriteLine($"Server URL: {serverUrl}");
                System.Diagnostics.Debug.WriteLine($"Code: {code?.Substring(0, Math.Min(10, code?.Length ?? 0))}...");
                System.Diagnostics.Debug.WriteLine($"Redirect URI: {redirectUri}");
                System.Diagnostics.Debug.WriteLine($"Code Verifier: {codeVerifier?.Substring(0, Math.Min(10, codeVerifier?.Length ?? 0))}...");

                var tokenRequest = new
                {
                    grant_type = "authorization_code",
                    client_id = "saga-mobile",
                    code = code,
                    redirect_uri = redirectUri,
                    code_verifier = codeVerifier
                };

                var json = JsonSerializer.Serialize(tokenRequest);
                System.Diagnostics.Debug.WriteLine($"Request payload: {json}");
                
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var callbackUrl = $"{serverUrl.TrimEnd('/')}/auth/openid/callback";
                System.Diagnostics.Debug.WriteLine($"Calling: {callbackUrl}");

                var response = await _httpClient.PostAsync(callbackUrl, content);

                System.Diagnostics.Debug.WriteLine($"Response status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Token exchange response: {responseContent}");
                    
                    // Try to deserialize as LoginResponse first (if Audiobookshelf returns this format)
                    try
                    {
                        var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, new JsonSerializerOptions 
                        { 
                            PropertyNameCaseInsensitive = true 
                        });
                        
                        if (loginResponse?.User != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"Successfully parsed as LoginResponse - User: {loginResponse.User.Username}");
                        }
                        
                        return loginResponse;
                    }
                    catch (Exception parseEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to parse as LoginResponse: {parseEx.Message}");
                        
                        // If that fails, try to parse as OAuth2 token response and convert
                        try
                        {
                            var tokenResponse = JsonSerializer.Deserialize<OAuthTokenResponse>(responseContent, new JsonSerializerOptions 
                            { 
                                PropertyNameCaseInsensitive = true 
                            });

                            if (tokenResponse?.AccessToken != null)
                            {
                                System.Diagnostics.Debug.WriteLine($"Parsed as OAuthTokenResponse - converting to LoginResponse");
                                
                                // Create a LoginResponse structure from the OAuth token
                                return new LoginResponse
                                {
                                    User = new User
                                    {
                                        Id = "oauth-user", // Will be replaced with actual user info
                                        Username = "oauth-user",
                                        Token = tokenResponse.AccessToken
                                    }
                                };
                            }
                        }
                        catch (Exception tokenParseEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to parse as OAuthTokenResponse: {tokenParseEx.Message}");
                        }
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Token exchange failed: {response.StatusCode} - {response.ReasonPhrase}");
                    System.Diagnostics.Debug.WriteLine($"Error content: {errorContent}");
                }

                System.Diagnostics.Debug.WriteLine("=== Token Exchange Failed ===");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"=== Token Exchange Exception ===");
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
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
