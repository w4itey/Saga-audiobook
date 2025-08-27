using Microsoft.Maui.Storage;
using Saga.Services;

namespace Saga;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        
        // Set default MainPage immediately to avoid NotImplementedException
        MainPage = new NavigationPage(new ServerDiscoveryPage());
        
        // Then check authentication and switch if needed
        _ = SetupInitialPageAsync();
    }

    private async Task SetupInitialPageAsync()
    {
        try
        {
            var authService = new AuthenticationService();
            var isAuthenticated = await authService.IsAuthenticatedAsync();
            var currentUser = await authService.GetCurrentUserAsync();
            
            if (isAuthenticated && currentUser != null)
            {
                // User is already logged in, switch to main page
                MainPage = new NavigationPage(new MainPage());
            }
            // If not authenticated, keep the default ServerDiscoveryPage
        }
        catch
        {
            // If there's any error with authentication check, keep the default ServerDiscoveryPage
        }
    }
}