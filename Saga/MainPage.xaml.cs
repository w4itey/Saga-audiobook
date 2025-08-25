using Saga.Services;
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
                var token = Preferences.Get("AuthToken", string.Empty);
                var serverUrl = Preferences.Get("ServerUrl", string.Empty);

                // REMOVE THIS BLOCK OF CODE
                // if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(serverUrl))
                // {
                //     // Not logged in, so navigate to login page
                //     Application.Current.MainPage = new LoginPage();
                //     return;
                // }

                var apiClient = new AudiobookshelfApiClient();
                var libraries = await apiClient.GetLibrariesAsync(serverUrl, token);

                if (libraries != null && libraries.Any())
                {
                    LibrariesCollectionView.ItemsSource = libraries;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
            finally
            {
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
            }
        }
    }
}
