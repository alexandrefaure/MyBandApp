using MyBandApp.Models.Entities;
using MyBandApp.Models.Presentation;
using MyBandApp.Views;

namespace MyBandApp.Services;

public interface IAppNavigationService
{
    Task NavigateToSongsAsync();
    Task NavigateToSetlistsAsync();
    Task OpenSongDetailAsync(Song song);
    Task OpenSetlistEditorAsync(SetlistWithSongs? setlist);
    Task CloseModalAsync();
}

public class MauiNavigationService : IAppNavigationService
{
    private readonly IServiceProvider _services;

    public MauiNavigationService(IServiceProvider services)
    {
        _services = services;
    }

    public Task NavigateToSongsAsync() => Shell.Current.GoToAsync("///SongsPage");

    public Task NavigateToSetlistsAsync() => Shell.Current.GoToAsync("///SetlistsPage");

    public async Task OpenSongDetailAsync(Song song)
    {
        var page = _services.GetRequiredService<SongDetailPopup>();
        page.ViewModel.Initialize(song);
        await PushModalAndWaitForDismissalAsync(page);
    }

    public async Task OpenSetlistEditorAsync(SetlistWithSongs? setlist)
    {
        var page = _services.GetRequiredService<SetlistEditPage>();
        await page.ViewModel.InitializeAsync(setlist);
        await PushModalAndWaitForDismissalAsync(page);
    }

    public Task CloseModalAsync() => Shell.Current.Navigation.PopModalAsync();

    private static async Task PushModalAndWaitForDismissalAsync(Page page)
    {
        var dismissed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        EventHandler? onDisappearing = null;
        onDisappearing = (_, _) =>
        {
            if (Shell.Current.Navigation.ModalStack.Contains(page))
                return;

            page.Disappearing -= onDisappearing;
            dismissed.TrySetResult();
        };
        page.Disappearing += onDisappearing;

        await Shell.Current.Navigation.PushModalAsync(page);
        await dismissed.Task;
    }
}
