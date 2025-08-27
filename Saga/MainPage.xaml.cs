using Saga.Services;
using Saga.Models;
using Microsoft.Maui.Storage;
using System;
using System.Linq;

namespace Saga
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }


        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                await DisplayAlert("MAINPAGE DEBUG", "OnAppearing started!", "OK");
                
                var token = Preferences.Get("AuthToken", string.Empty);
                var serverUrl = Preferences.Get("ServerUrl", string.Empty);

                await DisplayAlert("TOKEN DEBUG", $"Token: {(!string.IsNullOrEmpty(token) ? "Present" : "Empty")}\nServer: {serverUrl}", "OK");

                var apiClient = new AudiobookshelfApiClient();
                
                await DisplayAlert("API DEBUG", "About to call GetLibrariesAsync...", "OK");
                
                var libraries = await apiClient.GetLibrariesAsync(serverUrl, token);

                await DisplayAlert("RESPONSE DEBUG", $"Libraries response: {(libraries != null ? $"{libraries.Count} libraries" : "null")}", "OK");

                // Always add test data first to verify UI works
                var testLibraries = new List<Library>
                {
                    new Library { Id = "test1", Name = "Test Library 1" },
                    new Library { Id = "test2", Name = "Test Library 2" },
                    new Library { Id = "test3", Name = "Test Library 3" }
                };
                LibrariesCollectionView.ItemsSource = testLibraries;
                await DisplayAlert("TEST DATA DEBUG", "Added test libraries to verify UI", "OK");

                if (libraries != null && libraries.Any())
                {
                    LibrariesCollectionView.ItemsSource = libraries;
                    await DisplayAlert("SUCCESS DEBUG", $"Replaced with real data: {libraries.Count} libraries", "OK");
                }
                else
                {
                    await DisplayAlert("NO LIBRARIES DEBUG", "No real libraries found, keeping test data", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("EXCEPTION ERROR", $"An error occurred: {ex.Message}\n\nStack trace:\n{ex.StackTrace}", "OK");
            }
            finally
            {
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
                await DisplayAlert("FINAL DEBUG", "Loading indicator stopped", "OK");
            }
        }

        private async void OnLibrarySelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Library selectedLibrary)
            {
                await DisplayAlert("DEBUG", $"Selected library: {selectedLibrary.Name} (ID: {selectedLibrary.Id})", "OK");
                
                // Show loading indicator
                LoadingIndicator.IsRunning = true;
                LoadingIndicator.IsVisible = true;
                
                try
                {
                    var token = Preferences.Get("AuthToken", string.Empty);
                    var serverUrl = Preferences.Get("ServerUrl", string.Empty);
                    
                    var apiClient = new AudiobookshelfApiClient();
                    var books = await apiClient.GetLibraryItemsAsync(serverUrl, token, selectedLibrary.Id);
                    
                    await DisplayAlert("BOOKS DEBUG", $"Retrieved {books?.Count ?? 0} books from library", "OK");
                    
                    if (books != null && books.Any())
                    {
                        // For now, just show the book titles - later we'll create proper book models
                        var bookTitles = books.Select(b => new Library { Id = b.Id, Name = b.Media?.Metadata?.Title ?? "Unknown Title" }).ToList();
                        LibrariesCollectionView.ItemsSource = bookTitles;
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("ERROR", $"Error loading books: {ex.Message}", "OK");
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
