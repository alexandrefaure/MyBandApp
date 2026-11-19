using MyBandApp.Models.Entities;
using MyBandApp.Models.Presentation;
using MyBandApp.Services;
using MyBandApp.ViewModels;
using NUnit.Framework;

namespace MyBandApp.Tests.ViewModels;

[TestFixture]
public class SetlistsViewModelTests
{
    private sealed class FakeNavigationService : IAppNavigationService
    {
        public int OpenSetlistEditorCallCount { get; private set; }

        public Task NavigateToSongsAsync() => Task.CompletedTask;
        public Task NavigateToSetlistsAsync() => Task.CompletedTask;
        public Task OpenSongDetailAsync(Song song) => Task.CompletedTask;
        public Task OpenSetlistEditorAsync(SetlistWithSongs? setlist)
        {
            OpenSetlistEditorCallCount++;
            return Task.CompletedTask;
        }

        public Task CloseModalAsync() => Task.CompletedTask;
    }

    private sealed class FakeDialogService : IUserDialogService
    {
        public Task ShowAlertAsync(string title, string message, string cancel = "OK") => Task.CompletedTask;
        public Task<bool> ConfirmAsync(string title, string message, string accept, string cancel) => Task.FromResult(true);
    }

    [Test]
    public void Constructor_Should_CreateCommands()
    {
        var viewModel = new SetlistsViewModel(
            new SetlistService(null!),
            new FakeDialogService(),
            new FakeNavigationService());

        Assert.That(viewModel.LoadSetlistsCommand, Is.Not.Null);
        Assert.That(viewModel.AddSetlistCommand, Is.Not.Null);
        Assert.That(viewModel.EditSetlistCommand, Is.Not.Null);
        Assert.That(viewModel.DeleteSetlistCommand, Is.Not.Null);
    }

    [Test]
    public void SetlistsCollection_Should_AcceptItems()
    {
        var viewModel = new SetlistsViewModel(
            new SetlistService(null!),
            new FakeDialogService(),
            new FakeNavigationService());

        viewModel.Setlists.Add(new SetlistWithSongs
        {
            Setlist = new Setlist { Id = 1, Name = "Set 1", Date = new DateTime(2026, 1, 2) },
            Songs = new List<Song> { new() { Id = 10, Title = "Song A" } }
        });

        Assert.That(viewModel.Setlists, Has.Count.EqualTo(1));
        Assert.That(viewModel.Setlists[0].Name, Is.EqualTo("Set 1"));
    }
}
