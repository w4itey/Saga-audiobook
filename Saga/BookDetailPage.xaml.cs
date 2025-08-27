using Saga.Models;
using Saga.Services;
using System.Linq;

namespace Saga
{
    [QueryProperty(nameof(BookId), "bookId")]
    public partial class BookDetailPage : ContentPage
    {
        public string BookId { get; set; }

        private readonly AudiobookshelfApiClient _apiClient;
        private readonly IAuthenticationService _authService;
        private LibraryItem _book;

        public BookDetailPage()
        {
            InitializeComponent();
            _apiClient = new AudiobookshelfApiClient();
            _authService = new AuthenticationService();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadBookDetails();
        }

        private async Task LoadBookDetails()
        {
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

                _book = await _apiClient.GetLibraryItemAsync(serverUrl, token, BookId);

                if (_book != null)
                {
                    TitleLabel.Text = _book.Media.Metadata.Title;
                    SeriesLabel.Text = _book.Media.Metadata.SeriesName;
                    DescriptionLabel.Text = _book.Media.Metadata.Description;
                    CoverImage.Source = _book.Media.CoverPath;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private void OnPlayClicked(object sender, EventArgs e)
        {
            // Implementation for Play button
        }

        private void OnReadClicked(object sender, EventArgs e)
        {
            // Implementation for Read Ebook button
        }

        private void OnDownloadClicked(object sender, EventArgs e)
        {
            // Implementation for Download button
        }
    }
}
