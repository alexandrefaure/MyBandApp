using MyBandApp.Models.Entities;
using MyBandApp.Models.Presentation;
using MyBandApp.Services;
using MyBandApp.ViewModels;
using NUnit.Framework;

namespace MyBandApp.Tests.ViewModels;

[TestFixture]
public class SetlistEditViewModelTests
{
    private sealed class FakeNavigationService : IAppNavigationService
    {
        public Task NavigateToSongsAsync() => Task.CompletedTask;
        public Task NavigateToSetlistsAsync() => Task.CompletedTask;
        public Task OpenSongDetailAsync(Song song) => Task.CompletedTask;
        public Task OpenSetlistEditorAsync(SetlistWithSongs? setlist) => Task.CompletedTask;
        public Task CloseModalAsync() => Task.CompletedTask;
    }

    private sealed class FakeDialogService : IUserDialogService
    {
        public string? LastAlertTitle { get; private set; }
        public string? LastAlertMessage { get; private set; }

        public Task ShowAlertAsync(string title, string message, string cancel = "OK")
        {
            LastAlertTitle = title;
            LastAlertMessage = message;
            return Task.CompletedTask;
        }

        public Task<bool> ConfirmAsync(string title, string message, string accept, string cancel) => Task.FromResult(true);
    }

    [Test]
    public void Constructor_Should_CreateCommands()
    {
        var viewModel = new SetlistEditViewModel(
            new SetlistService(null!),
            new SongService(null!),
            new FakeDialogService(),
            new FakeNavigationService());

        Assert.That(viewModel.ShowSelectedSongsTabCommand, Is.Not.Null);
        Assert.That(viewModel.ShowCatalogueTabCommand, Is.Not.Null);
        Assert.That(viewModel.ToggleSongSelectionCommand, Is.Not.Null);
        Assert.That(viewModel.RemoveSongCommand, Is.Not.Null);
        Assert.That(viewModel.EditSongCommand, Is.Not.Null);
        Assert.That(viewModel.SaveCommand, Is.Not.Null);
        Assert.That(viewModel.CancelCommand, Is.Not.Null);
    }

    [Test]
    public void ReorderSelectedSongs_Should_MoveDraggedSongAndUpdateDisplayIndices()
    {
        var viewModel = new SetlistEditViewModel(
            new SetlistService(null!),
            new SongService(null!),
            new FakeDialogService(),
            new FakeNavigationService());

        var first = new SelectableSongModel { Song = new Song { Id = 1, Title = "Song A", DurationSeconds = 180 }, IsSelected = true };
        var second = new SelectableSongModel { Song = new Song { Id = 2, Title = "Song B", DurationSeconds = 240 }, IsSelected = true };

        viewModel.SelectedSongs.Add(first);
        viewModel.SelectedSongs.Add(second);
        first.DisplayIndex = 1;
        second.DisplayIndex = 2;

        viewModel.StartDragging(second);
        viewModel.ReorderSelectedSongs(first);

        Assert.That(viewModel.SelectedSongs[0].Song.Title, Is.EqualTo("Song B"));
        Assert.That(viewModel.SelectedSongs[0].DisplayIndex, Is.EqualTo(1));
        Assert.That(viewModel.SelectedSongs[1].Song.Title, Is.EqualTo("Song A"));
        Assert.That(viewModel.SelectedSongs[1].DisplayIndex, Is.EqualTo(2));
    }

    [Test]
    public void SaveAsync_Should_RejectEmptySetlistName()
    {
        var dialogs = new FakeDialogService();
        var viewModel = new SetlistEditViewModel(
            new SetlistService(null!),
            new SongService(null!),
            dialogs,
            new FakeNavigationService());

        viewModel.SetlistName = string.Empty;

        var method = typeof(SetlistEditViewModel).GetMethod("SaveAsync", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        var task = (Task)method.Invoke(viewModel, null)!;
        task.GetAwaiter().GetResult();

        Assert.That(dialogs.LastAlertTitle, Is.EqualTo("Erreur"));
        Assert.That(dialogs.LastAlertMessage, Is.EqualTo("Donne un nom à la setlist."));
    }
}
