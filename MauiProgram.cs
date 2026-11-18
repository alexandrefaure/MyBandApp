using Supabase;

namespace MyBandApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddSingleton<MyBandApp.Services.SongService>();
        builder.Services.AddTransient<MyBandApp.ViewModels.SongsViewModel>();
        builder.Services.AddTransient<MyBandApp.Views.SongsPage>();
        builder.Services.AddTransient<MyBandApp.Views.DashboardPage>();
        builder.Services.AddTransient<MyBandApp.Views.SetlistsPage>();

        // Configuration Supabase
        var url = "https://ttujmufkxsoxpfvqkabl.supabase.co";
        var key = "sb_publishable_psNwSZtY13UbosiZBNkJhA_182xuVdr";
        var options = new SupabaseOptions
        {
            AutoRefreshToken = true,
            AutoConnectRealtime = true
        };

        builder.Services.AddSingleton(provider => new Supabase.Client(url, key, options));

        return builder.Build();
    }
}
