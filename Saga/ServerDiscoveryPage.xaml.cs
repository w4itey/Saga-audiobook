using Saga.Services;
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace Saga
{
    public partial class ServerDiscoveryPage : ContentPage
    {
        private readonly AudiobookshelfApiClient _apiClient;

        public ServerDiscoveryPage()
        {
            InitializeComponent();
            _apiClient = new AudiobookshelfApiClient();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            // Check if we already have a server URL stored
            var storedServerUrl = await SecureStorage.GetAsync("server_url");
            if (!string.IsNullOrEmpty(storedServerUrl))
            {
                ServerUrlEntry.Text = storedServerUrl;
            }
        }

        private async void OnConnectButtonClicked(object sender, EventArgs e)
        {
            var serverUrl = ServerUrlEntry.Text?.Trim();
            
            if (string.IsNullOrWhiteSpace(serverUrl))
            {
                await DisplayAlert("Server URL Required", "Please enter your Audiobookshelf server URL.", "OK");
                return;
            }

            // Ensure URL has protocol
            if (!serverUrl.StartsWith("http://") && !serverUrl.StartsWith("https://"))
            {
                serverUrl = "https://" + serverUrl;
            }

            // Update UI to show loading
            ConnectButton.IsEnabled = false;
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            StatusLabel.Text = "Connecting to server...";
            StatusLabel.IsVisible = true;

            try
            {
                // Test connection and get server info
                var serverInfo = await _apiClient.GetServerInfoAsync(serverUrl);
                
                if (serverInfo != null)
                {
                    // Store the server URL
                    await SecureStorage.SetAsync("server_url", serverUrl);
                    
                    // Navigate to login page with server info
                    Application.Current.MainPage = new NavigationPage(new LoginPage(serverUrl, serverInfo));
                }
                else
                {
                    await DisplayAlert("Connection Failed", 
                        "Unable to connect to the Audiobookshelf server. Please check the URL and try again.", 
                        "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Connection Error", 
                    $"Failed to connect to server: {ex.Message}", 
                    "OK");
            }
            finally
            {
                // Reset UI
                ConnectButton.IsEnabled = true;
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
                StatusLabel.IsVisible = false;
            }
        }
    }
}