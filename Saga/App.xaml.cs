using Microsoft.Maui.Storage;

namespace Saga;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        var token = Preferences.Get("AuthToken", string.Empty);
        var serverUrl = Preferences.Get("ServerUrl", string.Empty);

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(serverUrl))
        {
            // Not logged in, so show the login page
            MainPage = new NavigationPage(new LoginPage());
        }
        else
        {
            // Already logged in, so show the main app shell
            MainPage = new AppShell();
        }
    }
}