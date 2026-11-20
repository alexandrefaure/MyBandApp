using Microsoft.Extensions.DependencyInjection;
using MyBandApp.Services;
using MyBandApp.ViewModels;
using MyBandApp.Views;

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

            var songsTask = songService.GetSongsAsync();
            var setlistsTask = setlistService.GetSetlistsWithSongsAsync();
            var pageWarmupTask = WarmupPagesAsync(services);

            await Task.WhenAll(songsTask, setlistsTask, pageWarmupTask);
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

    private static async Task WarmupPagesAsync(IServiceProvider services)
    {
        var songsPage = services.GetRequiredService<SongsPage>();
        var setlistsPage = services.GetRequiredService<SetlistsPage>();
        var songDetailPopup = services.GetRequiredService<SongDetailPopup>();
        var setlistEditPage = services.GetRequiredService<SetlistEditPage>();

        _ = services.GetRequiredService<DashboardPage>();
        _ = songsPage;
        _ = setlistsPage;
        _ = songDetailPopup;
        _ = setlistEditPage;

        var songsViewModel = services.GetRequiredService<SongsViewModel>();
        var setlistsViewModel = services.GetRequiredService<SetlistsViewModel>();

        await Task.WhenAll(
            songsViewModel.LoadSongsAsync(),
            setlistsViewModel.LoadSetlistsAsync());
    }
}
