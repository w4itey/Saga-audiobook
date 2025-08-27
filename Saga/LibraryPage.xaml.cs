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
            InitializeComponent();
            _apiClient = new AudiobookshelfApiClient();
            _authService = new AuthenticationService();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadLibraryItems();
        }

        private async Task LoadLibraryItems()
        {
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

                var shelves = await _apiClient.GetPersonalizedViewAsync(serverUrl, token, LibraryId);

                if (shelves != null)
                {
                    var continueReadingShelf = shelves.FirstOrDefault(s => s.Id == "continue-listening");
                    if (continueReadingShelf != null)
                    {
                        ContinueReadingCollectionView.ItemsSource = continueReadingShelf.Entities;
                    }

                    var continueSeriesShelf = shelves.FirstOrDefault(s => s.Id == "continue-series");
                    if (continueSeriesShelf != null)
                    {
                        UpNextCollectionView.ItemsSource = continueSeriesShelf.Entities;
                    }

                    var recentlyAddedShelf = shelves.FirstOrDefault(s => s.Id == "recently-added");
                    if (recentlyAddedShelf != null)
                    {
                        NewlyAddedCollectionView.ItemsSource = recentlyAddedShelf.Entities;
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
            finally
            {
                LoadingIndicator.IsVisible = false;
            }
        }

        private async void OnBookSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is LibraryItem selectedBook)
            {
                await Shell.Current.GoToAsync($"{nameof(BookDetailPage)}?bookId={selectedBook.Id}");
            }

            // Clear selection
            if(sender is CollectionView collectionView)
            {
                collectionView.SelectedItem = null;
            }
        }
    }
}
