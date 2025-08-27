using Saga.Models;
using Saga.Services;
using System.Linq;

namespace Saga
{
    [QueryProperty(nameof(LibraryId), "libraryId")]
    public partial class LibraryPage : ContentPage
    {
        public string LibraryId { get; set; }

        private readonly AudiobookshelfApiClient _apiClient;
        private readonly IAuthenticationService _authService;

        public LibraryPage()
        {
            System.Diagnostics.Debug.WriteLine("LibraryPage constructor called");
            try
            {
                InitializeComponent();
                System.Diagnostics.Debug.WriteLine("LibraryPage InitializeComponent completed");
                _apiClient = new AudiobookshelfApiClient();
                _authService = new AuthenticationService();
                System.Diagnostics.Debug.WriteLine("LibraryPage constructor completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LibraryPage constructor error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            System.Diagnostics.Debug.WriteLine($"LibraryPage OnAppearing - LibraryId: {LibraryId}");
            
            try
            {
                await LoadLibraryItems();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnAppearing: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                await DisplayAlert("Error", $"Failed to load library: {ex.Message}", "OK");
            }
        }

        private async Task LoadLibraryItems()
        {
            System.Diagnostics.Debug.WriteLine($"LoadLibraryItems started - LibraryId: {LibraryId}");
            
            if (LoadingIndicator != null)
                LoadingIndicator.IsVisible = true;
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                var token = await _authService.GetValidTokenAsync();
                var serverUrl = currentUser?.ServerUrls?.FirstOrDefault();

                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(serverUrl) || string.IsNullOrEmpty(LibraryId))
                {
                    await DisplayAlert("Authentication Error", "Authentication expired. Please login again.", "OK");
                    Application.Current.MainPage = new NavigationPage(new LoginPage());
                    return;
                }

                var shelves = await _apiClient.GetPersonalizedViewAsync(serverUrl, token, LibraryId);

                if (shelves != null)
                {
                    var continueReadingShelf = shelves.FirstOrDefault(s => s.Id == "continue-listening");
                    if (continueReadingShelf?.Entities != null && ContinueReadingCollectionView != null)
                    {
                        var items = continueReadingShelf.Entities.Where(e => e != null).ToList();
                        FixCoverPaths(items, serverUrl);
                        ContinueReadingCollectionView.ItemsSource = items;
                    }

                    var continueSeriesShelf = shelves.FirstOrDefault(s => s.Id == "continue-series");
                    if (continueSeriesShelf?.Entities != null && UpNextCollectionView != null)
                    {
                        var items = continueSeriesShelf.Entities.Where(e => e != null).ToList();
                        FixCoverPaths(items, serverUrl);
                        UpNextCollectionView.ItemsSource = items;
                    }

                    var recentlyAddedShelf = shelves.FirstOrDefault(s => s.Id == "recently-added");
                    if (recentlyAddedShelf?.Entities != null && NewlyAddedCollectionView != null)
                    {
                        var items = recentlyAddedShelf.Entities.Where(e => e != null).ToList();
                        FixCoverPaths(items, serverUrl);
                        NewlyAddedCollectionView.ItemsSource = items;
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
            finally
            {
                if (LoadingIndicator != null)
                    LoadingIndicator.IsVisible = false;
            }
        }

        private async void OnBookSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is LibraryItem selectedBook)
            {
                var bookDetailPage = new BookDetailPage();
                bookDetailPage.BookId = selectedBook.Id;
                await Navigation.PushAsync(bookDetailPage);
            }

            // Clear selection
            if(sender is CollectionView collectionView)
            {
                collectionView.SelectedItem = null;
            }
        }

        private void FixCoverPaths(List<LibraryItem> items, string serverUrl)
        {
            System.Diagnostics.Debug.WriteLine($"FixCoverPaths called with serverUrl: {serverUrl}");
            foreach (var item in items)
            {
                if (item?.Media != null && !string.IsNullOrEmpty(item.Id))
                {
                    System.Diagnostics.Debug.WriteLine($"Item: {item.Media.Metadata?.Title ?? "Unknown"}");
                    System.Diagnostics.Debug.WriteLine($"Original CoverPath: '{item.Media.CoverPath ?? "NULL"}'");
                    
                    // Always use the Audiobookshelf API endpoint for cover images
                    var originalPath = item.Media.CoverPath;
                    item.Media.CoverPath = $"{serverUrl.TrimEnd('/')}/api/items/{item.Id}/cover";
                    System.Diagnostics.Debug.WriteLine($"Set API CoverPath: '{originalPath}' -> '{item.Media.CoverPath}'");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Skipping item - Media: {item?.Media != null}, ID: '{item?.Id ?? "NULL"}'");
                }
            }
        }
    }
}
