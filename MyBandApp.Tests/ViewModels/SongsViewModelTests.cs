using MyBandApp.Models.Entities;
using MyBandApp.Models.Presentation;
using MyBandApp.Services;
using MyBandApp.ViewModels;
using NUnit.Framework;
using System.Reflection;

namespace MyBandApp.Tests.ViewModels;

[TestFixture]
public class SongsViewModelTests
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

        public Task<bool> ConfirmAsync(string title, string message, string accept, string cancel)
        {
            return Task.FromResult(true);
        }
    }

    [Test]
    public void Constructor_Should_CreateCommands()
    {
        var viewModel = new SongsViewModel(
            new SongService(null!),
            new FakeDialogService(),
            new FakeNavigationService());

        Assert.That(viewModel.LoadSongsCommand, Is.Not.Null);
        Assert.That(viewModel.ToggleAddFormCommand, Is.Not.Null);
        Assert.That(viewModel.AddSongCommand, Is.Not.Null);
        Assert.That(viewModel.DeleteSongCommand, Is.Not.Null);
        Assert.That(viewModel.OpenDetailCommand, Is.Not.Null);
    }

    [Test]
    public void SearchText_Should_FilterSongsByTitleArtistOrKey()
    {
        var viewModel = new SongsViewModel(
            new SongService(null!),
            new FakeDialogService(),
            new FakeNavigationService());

        var field = typeof(SongsViewModel).GetField("_allSongs", BindingFlags.Instance | BindingFlags.NonPublic)!;
        field.SetValue(viewModel, new List<SongListItem>
        {
            new(new Song { Id = 1, Title = "Get Lucky", Artist = "Daft Punk", Key = "F" }),
            new(new Song { Id = 2, Title = "Superstition", Artist = "Stevie Wonder", Key = "G" }),
            new(new Song { Id = 3, Title = "Africa", Artist = "Toto", Key = "Am" })
        });

        viewModel.SearchText = "stev";

        Assert.That(viewModel.FilteredSongs.Select(x => x.Title), Is.EquivalentTo(new[] { "Superstition" }));
    }

    [Test]
    public void ToggleAddFormCommand_Should_FlipFormVisibility()
    {
        var viewModel = new SongsViewModel(
            new SongService(null!),
            new FakeDialogService(),
            new FakeNavigationService());

        Assert.That(viewModel.IsAddFormVisible, Is.False);

        viewModel.ToggleAddFormCommand.Execute(null);

        Assert.That(viewModel.IsAddFormVisible, Is.True);
    }

    [Test]
    public void AddSongAsync_Should_RequireTitleAndArtist()
    {
        var dialogs = new FakeDialogService();
        var viewModel = new SongsViewModel(
            new SongService(null!),
            dialogs,
            new FakeNavigationService());

        viewModel.NewTitle = string.Empty;
        viewModel.NewArtist = string.Empty;

        var method = typeof(SongsViewModel).GetMethod("AddSongAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var task = (Task)method.Invoke(viewModel, null)!;
        task.GetAwaiter().GetResult();

        Assert.That(dialogs.LastAlertTitle, Is.EqualTo("Champs requis"));
    }
}
