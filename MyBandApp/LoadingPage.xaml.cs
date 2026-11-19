using Microsoft.Extensions.DependencyInjection;
using MyBandApp.Services;

namespace MyBandApp;

public partial class LoadingPage : ContentPage
{
    public LoadingPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            var services = Application.Current?.Handler?.MauiContext?.Services;
            if (services is null)
                return;

            var songService = services.GetRequiredService<SongService>();
            var setlistService = services.GetRequiredService<SetlistService>();

            await Task.WhenAll(
                songService.GetSongsAsync(),
                setlistService.GetSetlistsWithSongsAsync());
        }
        catch
        {
            // Keep the app usable even if the preload fails.
        }
        finally
        {
            var window = Application.Current?.Windows.FirstOrDefault();
            if (window is not null)
            {
                window.Page = new AppShell();
            }
        }
    }
}
