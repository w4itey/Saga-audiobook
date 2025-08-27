using Saga.Services;
using Saga.Models;
using Microsoft.Maui.Storage;
using System;
using System.Linq;

namespace Saga
{
    public partial class MainPage : ContentPage
    {
        private readonly IAuthenticationService _authService;

        public MainPage()
        {
            InitializeComponent();
            _authService = new AuthenticationService();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                // Check if user is authenticated
                var isAuthenticated = await _authService.IsAuthenticatedAsync();
                
                if (!isAuthenticated)
                {
                    // Redirect to server discovery page if not authenticated
                    Application.Current.MainPage = new NavigationPage(new ServerDiscoveryPage());
                    return;
                }

                var currentUser = await _authService.GetCurrentUserAsync();
                var token = await _authService.GetValidTokenAsync();
                
                if (currentUser == null || string.IsNullOrEmpty(token))
                {
                    await DisplayAlert("Authentication Error", "Unable to retrieve authentication information. Please login again.", "OK");
                    Application.Current.MainPage = new NavigationPage(new ServerDiscoveryPage());
                    return;
                }

                // Get server URL from user profile
                var serverUrl = currentUser.ServerUrls.FirstOrDefault();
                if (string.IsNullOrEmpty(serverUrl))
                {
                    await DisplayAlert("Configuration Error", "No server URL found. Please login again.", "OK");
                    Application.Current.MainPage = new NavigationPage(new LoginPage());
                    return;
                }

                var apiClient = new AudiobookshelfApiClient();
                var libraries = await apiClient.GetLibrariesAsync(serverUrl, token);

                if (libraries != null && libraries.Any())
                {
                    LibrariesCollectionView.ItemsSource = libraries;
                }
                else
                {
                    await DisplayAlert("No Libraries", "No libraries found on your Audiobookshelf server.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred while loading libraries: {ex.Message}", "OK");
            }
            finally
            {
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
            }
        }

        private async void OnLibrarySelected(object sender, SelectionChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("OnLibrarySelected called");
            try
            {
                System.Diagnostics.Debug.WriteLine($"Selection count: {e.CurrentSelection?.Count ?? -1}");
                var firstSelection = e.CurrentSelection?.FirstOrDefault();
                System.Diagnostics.Debug.WriteLine($"First selection type: {firstSelection?.GetType()?.Name ?? "NULL"}");
                
                if (firstSelection is Library selectedLibrary)
                {
                    System.Diagnostics.Debug.WriteLine($"Selected library: {selectedLibrary.Id}, Name: {selectedLibrary.Name}");
                    
                    // Use direct navigation since we're using NavigationPage architecture, not Shell
                    System.Diagnostics.Debug.WriteLine("Using direct navigation...");
                    var libraryPage = new LibraryPage();
                    libraryPage.LibraryId = selectedLibrary.Id;
                    System.Diagnostics.Debug.WriteLine($"Created LibraryPage with ID: {libraryPage.LibraryId}");
                    
                    await Navigation.PushAsync(libraryPage);
                    System.Diagnostics.Debug.WriteLine("Direct navigation completed successfully");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No library selected or selection is not a Library object");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OnLibrarySelected error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                await DisplayAlert("Navigation Error", $"Failed to navigate to library: {ex.Message}", "OK");
            }
            finally
            {
                // Clear selection
                if (LibrariesCollectionView != null)
                    LibrariesCollectionView.SelectedItem = null;
            }
        }
    }
}
