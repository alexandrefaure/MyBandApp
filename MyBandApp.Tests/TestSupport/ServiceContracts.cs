using MyBandApp.Models.Entities;
using MyBandApp.Models.Presentation;

namespace MyBandApp.Services;

public interface IAppNavigationService
{
    Task NavigateToSongsAsync();
    Task NavigateToSetlistsAsync();
    Task OpenSongDetailAsync(Song song);
    Task OpenSetlistEditorAsync(SetlistWithSongs? setlist);
    Task CloseModalAsync();
}

public interface IUserDialogService
{
    Task ShowAlertAsync(string title, string message, string cancel = "OK");
    Task<bool> ConfirmAsync(string title, string message, string accept, string cancel);
}
