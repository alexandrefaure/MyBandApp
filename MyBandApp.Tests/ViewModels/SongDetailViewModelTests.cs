using MyBandApp.Models.Entities;
using MyBandApp.Services;
using MyBandApp.ViewModels;
using NUnit.Framework;

namespace MyBandApp.Tests.ViewModels;

[TestFixture]
public class SongDetailViewModelTests
{
    private sealed class FakeNavigationService : IAppNavigationService
    {
        public Task NavigateToSongsAsync() => Task.CompletedTask;
        public Task NavigateToSetlistsAsync() => Task.CompletedTask;
        public Task OpenSongDetailAsync(Song song) => Task.CompletedTask;
        public Task OpenSetlistEditorAsync(MyBandApp.Models.Presentation.SetlistWithSongs? setlist) => Task.CompletedTask;
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
    public void Initialize_Should_PopulateSongProperties()
    {
        var viewModel = new SongDetailViewModel(
            new SongService(null!),
            new FakeDialogService(),
            new FakeNavigationService());

        var song = new Song
        {
            Id = 42,
            Title = "Song Title",
            Artist = "Artist Name",
            Key = "Am",
            Bpm = 118,
            DurationSeconds = 245,
            YoutubeLinks = "https://youtube.com/watch?v=abc"
        };

        viewModel.Initialize(song);

        Assert.That(viewModel.Title, Is.EqualTo("Song Title"));
        Assert.That(viewModel.Artist, Is.EqualTo("Artist Name"));
        Assert.That(viewModel.SelectedKey, Is.EqualTo("Am"));
        Assert.That(viewModel.EditableBpm, Is.EqualTo("118"));
        Assert.That(viewModel.EditableDuration, Is.EqualTo("04:05"));
        Assert.That(viewModel.EditableYoutubeLinks, Is.EqualTo("https://youtube.com/watch?v=abc"));
    }

    [Test]
    public void SaveAsync_Should_RejectInvalidBpm()
    {
        var dialogs = new FakeDialogService();
        var viewModel = new SongDetailViewModel(
            new SongService(null!),
            dialogs,
            new FakeNavigationService());

        var song = new Song
        {
            Id = 42,
            Title = "Song Title",
            Artist = "Artist Name",
            Bpm = 100,
            DurationSeconds = 180,
            Key = "C"
        };

        viewModel.Initialize(song);
        viewModel.EditableBpm = "500";

        var method = typeof(SongDetailViewModel).GetMethod("SaveAsync", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        var task = (Task)method.Invoke(viewModel, null)!;
        task.GetAwaiter().GetResult();

        Assert.That(dialogs.LastAlertTitle, Is.EqualTo("BPM invalide"));
    }
}
