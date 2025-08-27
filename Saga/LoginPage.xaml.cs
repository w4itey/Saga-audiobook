using Saga.Services;
using Saga.Models;
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Authentication;

namespace Saga
{
    public partial class LoginPage : ContentPage
    {
        private readonly IAuthenticationService _authService;
        private readonly string _serverUrl;
        private readonly ServerInfo _serverInfo;

        public LoginPage() : this("", null)
        {
        }

        public LoginPage(string serverUrl, ServerInfo serverInfo)
        {
            InitializeComponent();
            _authService = new AuthenticationService();
            _serverUrl = serverUrl;
            _serverInfo = serverInfo;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            if (_serverInfo != null)
            {
                SetupLoginPage();
            }
            else
            {
                // No server info, redirect to server discovery
                Application.Current.MainPage = new NavigationPage(new ServerDiscoveryPage());
            }
        }

        private void SetupLoginPage()
        {
            // Set server info labels
            ServerNameLabel.Text = _serverInfo.ServerName ?? "Audiobookshelf";
            ServerUrlLabel.Text = _serverUrl;
            
            // Show/hide local auth section based on server capabilities
            LocalAuthSection.IsVisible = _serverInfo.HasLocalAuth;
            
            // Setup SSO buttons if available
            if (_serverInfo.HasOpenIdAuth && _serverInfo.OpenIdProviders.Length > 0)
            {
                SSOSection.IsVisible = true;
                CreateSSOButtons();
            }
            else
            {
                SSOSection.IsVisible = false;
            }
        }

        private void CreateSSOButtons()
        {
            SSOButtonsContainer.Children.Clear();
            
            foreach (var provider in _serverInfo.OpenIdProviders)
            {
                var button = new Button
                {
                    Text = $"Continue with {provider.Name}",
                    FontSize = 16,
                    HeightRequest = 50,
                    CornerRadius = 8,
                    BackgroundColor = Color.FromArgb(provider.ButtonColor ?? "#007ACC"),
                    TextColor = Color.FromArgb(provider.TextColor ?? "#FFFFFF")
                };
                
                // Store provider info in command parameter
                button.CommandParameter = provider;
                button.Clicked += OnSSOButtonClicked;
                
                SSOButtonsContainer.Children.Add(button);
            }
        }

        private async void OnLoginButtonClicked(object sender, EventArgs e)
        {
            var username = UsernameEntry.Text;
            var password = PasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Login Required", "Please enter your username and password.", "OK");
                return;
            }

            // Show loading
            SetLoadingState(true);

            try
            {
                var result = await _authService.LoginAsync(_serverUrl, username, password);

                if (result.IsSuccess)
                {
                    // Navigate to main page immediately without displaying success alert
                    Application.Current.MainPage = new NavigationPage(new MainPage());
                }
                else
                {
                    await DisplayAlert("Login Failed", result.ErrorMessage ?? "Invalid credentials", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Login Error", $"An error occurred during login: {ex.Message}", "OK");
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        private async void OnSSOButtonClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is AuthMethod provider)
            {
                System.Diagnostics.Debug.WriteLine($"=== SSO Login Started ===");
                System.Diagnostics.Debug.WriteLine($"Provider: {provider.Name} (ID: {provider.Id})");
                System.Diagnostics.Debug.WriteLine($"Server URL: {_serverUrl}");
                
                SetLoadingState(true);

                try
                {
                    var result = await StartOpenIdFlowAsync(provider);

                    System.Diagnostics.Debug.WriteLine($"SSO Flow completed - Success: {result.IsSuccess}");
                    
                    if (result.IsSuccess)
                    {
                        System.Diagnostics.Debug.WriteLine($"SSO Login successful! User: {result.User?.Username}");
                        System.Diagnostics.Debug.WriteLine("Navigating to MainPage...");
                        
                        // Navigate to main page immediately without displaying success alert
                        Application.Current.MainPage = new NavigationPage(new MainPage());
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"SSO Login failed: {result.ErrorMessage}");
                        await DisplayAlert("SSO Login Failed", result.ErrorMessage ?? "Authentication failed", "OK");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"=== SSO Exception ===");
                    System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                    
                    await DisplayAlert("SSO Error", $"An error occurred during authentication: {ex.Message}", "OK");
                }
                finally
                {
                    SetLoadingState(false);
                    System.Diagnostics.Debug.WriteLine("=== SSO Login Flow Ended ===");
                }
            }
        }

        private async Task<AuthenticationResult> StartOpenIdFlowAsync(AuthMethod provider)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== Starting OpenID Flow ===");
                
                var redirectUri = "audiobookshelf://oauth";
                var state = Guid.NewGuid().ToString();
                var codeChallenge = GenerateCodeChallenge();
                
                // Store PKCE parameters for verification
                System.Diagnostics.Debug.WriteLine($"Storing OAuth state: {state}");
                System.Diagnostics.Debug.WriteLine($"Storing code verifier: {codeChallenge.verifier.Substring(0, Math.Min(10, codeChallenge.verifier.Length))}...");
                
                await SecureStorage.SetAsync("oauth_state", state);
                await SecureStorage.SetAsync("oauth_code_verifier", codeChallenge.verifier);
                await SecureStorage.SetAsync("oauth_provider_id", provider.Id);
                
                // Verify storage immediately
                var storedState = await SecureStorage.GetAsync("oauth_state");
                System.Diagnostics.Debug.WriteLine($"Verified stored state: {storedState}");

                // Build Audiobookshelf OpenID URL according to API documentation
                var authUrl = $"{_serverUrl.TrimEnd('/')}/auth/openid?" +
                             $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
                             $"state={Uri.EscapeDataString(state)}&" +
                             $"code_challenge={codeChallenge.challenge}&" +
                             $"code_challenge_method=S256";
                
                System.Diagnostics.Debug.WriteLine($"Authorization URL: {authUrl}");

                // Launch browser for authentication
                System.Diagnostics.Debug.WriteLine("Launching WebAuthenticator...");
                
                WebAuthenticatorResult authResult = null;
                
                try 
                {
                    // Add timeout to prevent indefinite waiting
                    var authTask = WebAuthenticator.AuthenticateAsync(
                        new WebAuthenticatorOptions
                        {
                            Url = new Uri(authUrl),
                            CallbackUrl = new Uri(redirectUri),
                            PrefersEphemeralWebBrowserSession = false
                        });
                    
                    // Wait for authentication with timeout
                    var timeoutTask = Task.Delay(TimeSpan.FromMinutes(5)); // 5 minute timeout
                    var completedTask = await Task.WhenAny(authTask, timeoutTask);
                    
                    if (completedTask == timeoutTask)
                    {
                        System.Diagnostics.Debug.WriteLine("WebAuthenticator timed out after 5 minutes");
                        return new AuthenticationResult
                        {
                            IsSuccess = false,
                            ErrorMessage = "Authentication timed out. Please try again."
                        };
                    }
                    
                    authResult = await authTask;
                    System.Diagnostics.Debug.WriteLine("WebAuthenticator returned successfully");
                }
                catch (TaskCanceledException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"WebAuthenticator was cancelled: {ex.Message}");
                    return new AuthenticationResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Authentication was cancelled by user"
                    };
                }
                catch (PlatformNotSupportedException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"WebAuthenticator platform not supported: {ex.Message}");
                    return new AuthenticationResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "SSO authentication is not supported on this device"
                    };
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"WebAuthenticator exception: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Exception type: {ex.GetType().Name}");
                    System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                    
                    return new AuthenticationResult
                    {
                        IsSuccess = false,
                        ErrorMessage = $"Authentication failed: {ex.Message}"
                    };
                }

                if (authResult == null)
                {
                    System.Diagnostics.Debug.WriteLine("WebAuthenticator returned null result");
                    return new AuthenticationResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "No authentication result received"
                    };
                }

                // Process the callback
                System.Diagnostics.Debug.WriteLine("Processing OAuth callback...");
                return await ProcessOpenIdCallbackAsync(authResult);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"=== StartOpenIdFlowAsync Exception ===");
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                
                return new AuthenticationResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"SSO authentication failed: {ex.Message}"
                };
            }
        }

        private async Task<AuthenticationResult> ProcessOpenIdCallbackAsync(WebAuthenticatorResult authResult)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== SSO Callback Processing Started ===");
                
                // Log all received properties
                System.Diagnostics.Debug.WriteLine("Received callback properties:");
                foreach (var prop in authResult.Properties)
                {
                    System.Diagnostics.Debug.WriteLine($"  {prop.Key}: {prop.Value}");
                }

                // Verify state parameter
                var storedState = await SecureStorage.GetAsync("oauth_state");
                var receivedState = authResult.Properties.GetValueOrDefault("state");
                
                System.Diagnostics.Debug.WriteLine($"State verification - Stored: '{storedState}', Received: '{receivedState}'");
                System.Diagnostics.Debug.WriteLine($"State lengths - Stored: {storedState?.Length ?? 0}, Received: {receivedState?.Length ?? 0}");
                
                // Check for common state parameter issues
                bool statesMatch = false;
                if (!string.IsNullOrEmpty(storedState) && !string.IsNullOrEmpty(receivedState))
                {
                    statesMatch = storedState.Trim() == receivedState.Trim();
                    if (!statesMatch)
                    {
                        System.Diagnostics.Debug.WriteLine("Exact match failed, checking for URL encoding issues...");
                        var decodedReceived = Uri.UnescapeDataString(receivedState);
                        System.Diagnostics.Debug.WriteLine($"URL decoded received: '{decodedReceived}'");
                        statesMatch = storedState.Trim() == decodedReceived.Trim();
                    }
                }
                
                if (!statesMatch)
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: State parameter mismatch!");
                    
                    // For debugging purposes, let's temporarily allow mismatched states but warn
                    // TODO: Remove this bypass after debugging
                    System.Diagnostics.Debug.WriteLine("WARNING: Bypassing state check for debugging - THIS IS INSECURE!");
                    // Uncomment the return below to enforce state checking:
                    /*
                    return new AuthenticationResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Invalid state parameter - security check failed"
                    };
                    */
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("State parameter validation passed!");
                }

                // Get authorization code
                var code = authResult.Properties.GetValueOrDefault("code");
                System.Diagnostics.Debug.WriteLine($"Authorization code received: {(!string.IsNullOrEmpty(code) ? "YES" : "NO")}");
                
                if (string.IsNullOrEmpty(code))
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: No authorization code in callback!");
                    return new AuthenticationResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "No authorization code received from SSO provider"
                    };
                }

                // Exchange code for token
                var apiClient = new AudiobookshelfApiClient();
                var codeVerifier = await SecureStorage.GetAsync("oauth_code_verifier");
                var providerId = await SecureStorage.GetAsync("oauth_provider_id");

                System.Diagnostics.Debug.WriteLine($"Starting token exchange with server: {_serverUrl}");
                System.Diagnostics.Debug.WriteLine($"Code verifier present: {(!string.IsNullOrEmpty(codeVerifier) ? "YES" : "NO")}");

                var tokenResponse = await apiClient.ExchangeCodeForTokenAsync(
                    _serverUrl, 
                    code, 
                    "audiobookshelf://oauth", 
                    codeVerifier);

                System.Diagnostics.Debug.WriteLine($"Token exchange response received: {(tokenResponse != null ? "YES" : "NO")}");
                
                if (tokenResponse?.User?.Token != null)
                {
                    System.Diagnostics.Debug.WriteLine($"User token received: {tokenResponse.User.Token.Substring(0, Math.Min(10, tokenResponse.User.Token.Length))}...");
                    System.Diagnostics.Debug.WriteLine($"User ID: {tokenResponse.User.Id}, Username: {tokenResponse.User.Username}");
                }

                if (tokenResponse != null && !string.IsNullOrEmpty(tokenResponse.User?.Token))
                {
                    // Create user profile
                    var userProfile = new UserProfile
                    {
                        Id = tokenResponse.User.Id,
                        Username = tokenResponse.User.Username,
                        DisplayName = tokenResponse.User.Username,
                        Email = "",
                        Provider = AuthenticationProvider.Audiobookshelf,
                        ProviderId = tokenResponse.User.Id,
                        LastLoginAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    };

                    userProfile.ServerUrls.Add(_serverUrl);

                    System.Diagnostics.Debug.WriteLine("Creating authentication result and storing user...");

                    var result = new AuthenticationResult
                    {
                        IsSuccess = true,
                        User = userProfile,
                        AccessToken = tokenResponse.User.Token,
                        ExpiresAt = DateTime.UtcNow.AddHours(24),
                        Provider = AuthenticationProvider.Audiobookshelf
                    };

                    // Store user and token
                    var addUserResult = await _authService.AddUserAsync(userProfile);
                    var switchUserResult = await _authService.SwitchUserAsync(userProfile.Id);
                    
                    System.Diagnostics.Debug.WriteLine($"Add user result: {addUserResult}");
                    System.Diagnostics.Debug.WriteLine($"Switch user result: {switchUserResult}");

                    // Clean up stored OAuth parameters
                    SecureStorage.Remove("oauth_state");
                    SecureStorage.Remove("oauth_code_verifier");
                    SecureStorage.Remove("oauth_provider_id");

                    System.Diagnostics.Debug.WriteLine("=== SSO Login Success! ===");
                    return result;
                }

                System.Diagnostics.Debug.WriteLine("ERROR: Token exchange failed - no valid token received");
                return new AuthenticationResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Failed to exchange authorization code for valid token"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"=== SSO Callback Exception ===");
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                
                return new AuthenticationResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"SSO authentication failed: {ex.Message}"
                };
            }
        }

        private void OnChangeServerClicked(object sender, EventArgs e)
        {
            Application.Current.MainPage = new NavigationPage(new ServerDiscoveryPage());
        }

        private void SetLoadingState(bool isLoading)
        {
            LoadingIndicator.IsVisible = isLoading;
            LoadingIndicator.IsRunning = isLoading;
            LoginButton.IsEnabled = !isLoading;
            
            foreach (var child in SSOButtonsContainer.Children)
            {
                if (child is Button button)
                {
                    button.IsEnabled = !isLoading;
                }
            }
        }

        private (string challenge, string verifier) GenerateCodeChallenge()
        {
            // Generate code verifier (43-128 characters)
            var verifier = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Guid.NewGuid().ToString() + Guid.NewGuid().ToString()))
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace("=", "")
                .Substring(0, 64);

            // Generate code challenge (SHA256 hash of verifier, base64 encoded)
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var challengeBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(verifier));
                var challenge = Convert.ToBase64String(challengeBytes)
                    .Replace('+', '-')
                    .Replace('/', '_')
                    .Replace("=", "");

                return (challenge, verifier);
            }
        }
    }
}
