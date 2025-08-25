using Audiobookshelf.Mobile.Services;
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace Audiobookshelf.Mobile
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private async void OnLoginButtonClicked(object sender, EventArgs e)
        {
            var apiClient = new AudiobookshelfApiClient();
            var serverUrl = ServerUrlEntry.Text;
            var username = UsernameEntry.Text;
            var password = PasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(serverUrl) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Login Failed", "Please enter a server URL, username, and password.", "OK");
                return;
            }

            try
            {
                var loginResponse = await apiClient.LoginAsync(serverUrl, username, password);

                if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.User?.Token))
                {
                    // Store the token (insecurely for now)
                    // In a real app, use secure storage
                    Preferences.Set("AuthToken", loginResponse.User.Token);
                    Preferences.Set("ServerUrl", serverUrl);

                    // Navigate to the main page
                    Application.Current.MainPage = new AppShell();
                }
                else
                {
                    // Show an error message
                    await DisplayAlert("Login Failed", "Invalid server URL, username, or password.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Login Error", $"An error occurred during login: {ex.Message}", "OK");
            }
        }
    }
}
