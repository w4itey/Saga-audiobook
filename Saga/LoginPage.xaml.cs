using Saga.Services;
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace Saga
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

            await DisplayAlert("Debug", $"Login attempt:\nServer: {serverUrl}\nUsername: {username}\nPassword: {(!string.IsNullOrEmpty(password) ? "[PROVIDED]" : "[EMPTY]")}", "OK");

            if (string.IsNullOrWhiteSpace(serverUrl) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Login Failed", "Please enter a server URL, username, and password.", "OK");
                return;
            }

            try
            {
                await DisplayAlert("Debug", "Making API call...", "OK");
                var loginResponse = await apiClient.LoginAsync(serverUrl, username, password);

                await DisplayAlert("Debug", $"API Response: {(loginResponse != null ? "Response received" : "NULL response")}", "OK");

                if (loginResponse != null)
                {
                    await DisplayAlert("Debug", $"User: {(loginResponse.User != null ? "Present" : "NULL")}\nToken: {(!string.IsNullOrEmpty(loginResponse.User?.Token) ? "Present" : "NULL/Empty")}", "OK");
                }

                if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.User?.Token))
                {
                    // Store the token (insecurely for now)
                    // In a real app, use secure storage
                    Preferences.Set("AuthToken", loginResponse.User.Token);
                    Preferences.Set("ServerUrl", serverUrl);

                    await DisplayAlert("Debug", "Token saved, navigating to MainPage directly...", "OK");

                    // Navigate directly to MainPage instead of AppShell
                    Application.Current.MainPage = new NavigationPage(new MainPage());
                }
                else
                {
                    // Show an error message
                    await DisplayAlert("Login Failed", "Invalid server URL, username, or password.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Login Error", $"An error occurred during login: {ex.Message}\n\nStack trace:\n{ex.StackTrace}", "OK");
            }
        }
    }
}
