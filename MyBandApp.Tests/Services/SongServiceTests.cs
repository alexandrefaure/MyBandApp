using MyBandApp.Models.Entities;
using MyBandApp.Services;
using NUnit.Framework;

namespace MyBandApp.Tests.Services;

[TestFixture]
public class SongServiceTests
{
    [Test]
    public void Constructor_Should_CreateServiceInstance()
    {
        var service = new SongService(null!);

        Assert.That(service, Is.Not.Null);
    }

    [Test]
    public void Service_Should_ExposeExpectedOperations()
    {
        var methodNames = typeof(SongService).GetMethods().Select(x => x.Name).ToHashSet();

        Assert.That(methodNames, Does.Contain(nameof(SongService.GetSongsAsync)));
        Assert.That(methodNames, Does.Contain(nameof(SongService.AddSongAsync)));
        Assert.That(methodNames, Does.Contain(nameof(SongService.DeleteSongAsync)));
        Assert.That(methodNames, Does.Contain(nameof(SongService.UpdateSongAsync)));
    }

    [Test]
    public void SongEntity_Should_InitializeWithDefaultValues()
    {
        var song = new Song();

        Assert.That(song.Id, Is.EqualTo(0));
        Assert.That(song.Title, Is.EqualTo(string.Empty));
        Assert.That(song.Artist, Is.EqualTo(string.Empty));
        Assert.That(song.Key, Is.EqualTo(string.Empty));
        Assert.That(song.Bpm, Is.EqualTo(0));
        Assert.That(song.DurationSeconds, Is.EqualTo(0));
        Assert.That(song.YoutubeLinks, Is.EqualTo(string.Empty));
    }
}
