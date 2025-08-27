using Saga.Models;
using System.Text.Json;

namespace Saga.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly AudiobookshelfApiClient _apiClient;
        private MultiUserSession _currentSession;

        public event EventHandler<AuthenticationResult> AuthenticationChanged;
        public event EventHandler<UserProfile> UserSwitched;

        public AuthenticationService()
        {
            _apiClient = new AudiobookshelfApiClient();
            _currentSession = new MultiUserSession();
        }

        public async Task<AuthenticationResult> LoginAsync(string serverUrl, string username, string password)
        {
            try
            {
                var loginResponse = await _apiClient.LoginAsync(serverUrl, username, password);
                
                if (loginResponse?.User != null && !string.IsNullOrEmpty(loginResponse.User.Token))
                {
                    var userProfile = new UserProfile
                    {
                        Id = loginResponse.User.Id,
                        Username = loginResponse.User.Username,
                        DisplayName = loginResponse.User.Username,
                        Email = "", // No email in Audiobookshelf User model
                        Provider = AuthenticationProvider.Audiobookshelf,
                        ProviderId = loginResponse.User.Id,
                        LastLoginAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    };

                    userProfile.ServerUrls.Add(serverUrl);

                    var result = new AuthenticationResult
                    {
                        IsSuccess = true,
                        User = userProfile,
                        AccessToken = loginResponse.User.Token,
                        ExpiresAt = DateTime.UtcNow.AddHours(24), // Default 24h expiration
                        Provider = AuthenticationProvider.Audiobookshelf
                    };

                    await StoreUserTokenAsync(userProfile.Id, loginResponse.User.Token, serverUrl);
                    await UpdateCurrentSessionAsync(userProfile);

                    AuthenticationChanged?.Invoke(this, result);
                    return result;
                }

                return new AuthenticationResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Invalid credentials"
                };
            }
            catch (Exception ex)
            {
                return new AuthenticationResult
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<AuthenticationResult> LoginWithSSOAsync(AuthenticationProvider provider)
        {
            try
            {
                var serverUrl = await SecureStorage.GetAsync("server_url");
                if (string.IsNullOrEmpty(serverUrl))
                {
                    return new AuthenticationResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Server URL not configured"
                    };
                }

                var redirectUri = "saga://auth/callback";
                var authUrl = await _apiClient.StartSSOAuthAsync(serverUrl, redirectUri);

                // Note: In a real implementation, this would launch a web browser
                // and handle the callback. For now, we'll return a partial result
                // indicating that the SSO flow has started.
                
                return new AuthenticationResult
                {
                    IsSuccess = false,
                    ErrorMessage = "SSO flow initiated - implementation pending"
                };
            }
            catch (Exception ex)
            {
                return new AuthenticationResult
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<bool> LogoutAsync()
        {
            try
            {
                if (_currentSession?.CurrentUser != null)
                {
                    // Remove current user's token
                    await SecureStorage.SetAsync($"user_token_{_currentSession.CurrentUser.Id}", string.Empty);
                    
                    // Clear current session
                    _currentSession.CurrentUser = null;
                    _currentSession.PrimaryUserId = null;
                    
                    // Fire event
                    AuthenticationChanged?.Invoke(this, new AuthenticationResult { IsSuccess = false });
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RefreshTokenAsync()
        {
            // TODO: Implement token refresh logic when Audiobookshelf supports it
            return false;
        }

        public async Task<UserProfile> GetCurrentUserAsync()
        {
            // Ensure current session is loaded
            await LoadCurrentSessionAsync();
            return _currentSession?.CurrentUser;
        }

        public async Task<bool> UpdateUserProfileAsync(UserProfile user)
        {
            try
            {
                var userJson = JsonSerializer.Serialize(user);
                await SecureStorage.SetAsync($"user_profile_{user.Id}", userJson);
                
                // Update current session if this is the current user
                if (_currentSession?.CurrentUser?.Id == user.Id)
                {
                    _currentSession.CurrentUser = user;
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<UserProfile>> GetAvailableUsersAsync()
        {
            try
            {
                var users = new List<UserProfile>();
                var userListJson = await SecureStorage.GetAsync("available_users");
                
                if (!string.IsNullOrEmpty(userListJson))
                {
                    var userIds = JsonSerializer.Deserialize<List<string>>(userListJson);
                    
                    foreach (var userId in userIds)
                    {
                        var userJson = await SecureStorage.GetAsync($"user_profile_{userId}");
                        if (!string.IsNullOrEmpty(userJson))
                        {
                            var user = JsonSerializer.Deserialize<UserProfile>(userJson);
                            users.Add(user);
                        }
                    }
                }
                
                return users;
            }
            catch
            {
                return new List<UserProfile>();
            }
        }

        public async Task<bool> SwitchUserAsync(string userId)
        {
            try
            {
                var users = await GetAvailableUsersAsync();
                var targetUser = users.FirstOrDefault(u => u.Id == userId);
                
                if (targetUser != null)
                {
                    _currentSession.CurrentUser = targetUser;
                    _currentSession.PrimaryUserId = userId;
                    
                    UserSwitched?.Invoke(this, targetUser);
                    return true;
                }
                
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AddUserAsync(UserProfile user)
        {
            try
            {
                var users = await GetAvailableUsersAsync();
                if (!users.Any(u => u.Id == user.Id))
                {
                    users.Add(user);
                    
                    var userIds = users.Select(u => u.Id).ToList();
                    var userListJson = JsonSerializer.Serialize(userIds);
                    await SecureStorage.SetAsync("available_users", userListJson);
                    
                    await UpdateUserProfileAsync(user);
                    
                    // Update current session
                    _currentSession.AvailableUsers = users;
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveUserAsync(string userId)
        {
            try
            {
                var users = await GetAvailableUsersAsync();
                users.RemoveAll(u => u.Id == userId);
                
                var userIds = users.Select(u => u.Id).ToList();
                var userListJson = JsonSerializer.Serialize(userIds);
                await SecureStorage.SetAsync("available_users", userListJson);
                
                // Remove user data
                SecureStorage.Remove($"user_profile_{userId}");
                SecureStorage.Remove($"user_token_{userId}");
                
                // Update current session
                _currentSession.AvailableUsers = users;
                
                // If removing current user, logout
                if (_currentSession?.CurrentUser?.Id == userId)
                {
                    await LogoutAsync();
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> GetValidTokenAsync()
        {
            // Ensure current session is loaded
            await LoadCurrentSessionAsync();
            
            if (_currentSession?.CurrentUser != null)
            {
                var token = await SecureStorage.GetAsync($"user_token_{_currentSession.CurrentUser.Id}");
                return token;
            }
            
            return null;
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var token = await GetValidTokenAsync();
            return !string.IsNullOrEmpty(token);
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            // TODO: Implement token validation with Audiobookshelf server
            return !string.IsNullOrEmpty(token);
        }

        public async Task<MultiUserSession> GetCurrentSessionAsync()
        {
            return _currentSession;
        }

        public async Task<bool> UpdateSessionAsync(MultiUserSession session)
        {
            _currentSession = session;
            return true;
        }

        private async Task<bool> StoreUserTokenAsync(string userId, string token, string serverUrl)
        {
            try
            {
                await SecureStorage.SetAsync($"user_token_{userId}", token);
                await SecureStorage.SetAsync($"user_server_{userId}", serverUrl);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> UpdateCurrentSessionAsync(UserProfile user)
        {
            try
            {
                _currentSession.CurrentUser = user;
                _currentSession.PrimaryUserId = user.Id;
                
                // Add to available users if not already present
                await AddUserAsync(user);
                
                // Persist current session
                await PersistCurrentSessionAsync();
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task LoadCurrentSessionAsync()
        {
            try
            {
                // Only load if session is empty
                if (_currentSession?.CurrentUser != null) return;
                
                var currentUserId = await SecureStorage.GetAsync("current_user_id");
                if (!string.IsNullOrEmpty(currentUserId))
                {
                    // Load user profile
                    var userProfileJson = await SecureStorage.GetAsync($"user_profile_{currentUserId}");
                    if (!string.IsNullOrEmpty(userProfileJson))
                    {
                        var userProfile = JsonSerializer.Deserialize<UserProfile>(userProfileJson);
                        if (userProfile != null)
                        {
                            _currentSession.CurrentUser = userProfile;
                            _currentSession.PrimaryUserId = userProfile.Id;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadCurrentSessionAsync error: {ex.Message}");
            }
        }

        private async Task PersistCurrentSessionAsync()
        {
            try
            {
                if (_currentSession?.CurrentUser != null)
                {
                    await SecureStorage.SetAsync("current_user_id", _currentSession.CurrentUser.Id);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PersistCurrentSessionAsync error: {ex.Message}");
            }
        }
    }
}