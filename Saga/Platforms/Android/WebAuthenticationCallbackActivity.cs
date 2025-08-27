using Android.App;
using Android.Content.PM;
using Android.Content;

namespace Saga
{
    [Activity(
        NoHistory = true, 
        LaunchMode = LaunchMode.SingleTop, 
        Exported = true)]
    [IntentFilter(
        new[] { Intent.ActionView },
        Categories = new[] { 
            Intent.CategoryDefault, 
            Intent.CategoryBrowsable 
        },
        DataScheme = "audiobookshelf")]
    public class WebAuthenticationCallbackActivity : Microsoft.Maui.Authentication.WebAuthenticatorCallbackActivity
    {
    }
}