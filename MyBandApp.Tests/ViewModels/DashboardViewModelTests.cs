using MyBandApp.Services;
using MyBandApp.ViewModels;
using NUnit.Framework;

namespace MyBandApp.Tests.ViewModels;

[TestFixture]
public class DashboardViewModelTests
{
    private sealed class FakeNavigationService : IAppNavigationService
    {
        public int NavigateToSongsCallCount { get; private set; }
        public int NavigateToSetlistsCallCount { get; private set; }

        public Task NavigateToSongsAsync()
        {
            NavigateToSongsCallCount++;
            return Task.CompletedTask;
        }

        public Task NavigateToSetlistsAsync()
        {
            NavigateToSetlistsCallCount++;
            return Task.CompletedTask;
        }

        public Task OpenSongDetailAsync(MyBandApp.Models.Entities.Song song) => Task.CompletedTask;
        public Task OpenSetlistEditorAsync(MyBandApp.Models.Presentation.SetlistWithSongs? setlist) => Task.CompletedTask;
        public Task CloseModalAsync() => Task.CompletedTask;
    }

    [Test]
    public void Constructor_Should_CreateCommands()
    {
        var navigation = new FakeNavigationService();
        var viewModel = new DashboardViewModel(navigation);

        Assert.That(viewModel.GoToSongsCommand, Is.Not.Null);
        Assert.That(viewModel.GoToSetlistsCommand, Is.Not.Null);
    }

    [Test]
    public void GoToSongsCommand_Should_CallNavigationService()
    {
        var navigation = new FakeNavigationService();
        var viewModel = new DashboardViewModel(navigation);

        viewModel.GoToSongsCommand.Execute(null);

        Assert.That(navigation.NavigateToSongsCallCount, Is.EqualTo(1));
    }

    [Test]
    public void GoToSetlistsCommand_Should_CallNavigationService()
    {
        var navigation = new FakeNavigationService();
        var viewModel = new DashboardViewModel(navigation);

        viewModel.GoToSetlistsCommand.Execute(null);

        Assert.That(navigation.NavigateToSetlistsCallCount, Is.EqualTo(1));
    }
}
