using Microsoft.Extensions.Logging;
using Saga.Services;

namespace Saga;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>();

		// Register services
		builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();
		builder.Services.AddTransient<AudiobookshelfApiClient>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
