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
            if (e.CurrentSelection.FirstOrDefault() is Library selectedLibrary)
            {
                // Show loading indicator
                LoadingIndicator.IsRunning = true;
                LoadingIndicator.IsVisible = true;
                
                try
                {
                    var currentUser = await _authService.GetCurrentUserAsync();
                    var token = await _authService.GetValidTokenAsync();
                    var serverUrl = currentUser?.ServerUrls.FirstOrDefault();

                    if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(serverUrl))
                    {
                        await DisplayAlert("Authentication Error", "Authentication expired. Please login again.", "OK");
                        Application.Current.MainPage = new NavigationPage(new LoginPage());
                        return;
                    }
                    
                    var apiClient = new AudiobookshelfApiClient();
                    var books = await apiClient.GetLibraryItemsAsync(serverUrl, token, selectedLibrary.Id);
                    
                    if (books != null && books.Any())
                    {
                        // For now, just show the book titles - later we'll create proper book models
                        var bookTitles = books.Select(b => new Library { 
                            Id = b.Id, 
                            Name = b.Media?.Metadata?.Title ?? "Unknown Title" 
                        }).ToList();
                        LibrariesCollectionView.ItemsSource = bookTitles;
                    }
                    else
                    {
                        await DisplayAlert("No Books", $"No books found in library '{selectedLibrary.Name}'.", "OK");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Error loading books: {ex.Message}", "OK");
                }
                finally
                {
                    LoadingIndicator.IsRunning = false;
                    LoadingIndicator.IsVisible = false;
                }
            }
            
            // Clear selection
            LibrariesCollectionView.SelectedItem = null;
        }
    }
}
