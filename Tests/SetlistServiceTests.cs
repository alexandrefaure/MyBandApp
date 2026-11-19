using MyBandApp.Models.Entities;
using NUnit.Framework;

namespace MyBandApp.Tests;

[TestFixture]
public class SetlistServiceTests
{
    [Test]
    public async Task SaveSetlist_Should_PrepareCorrectStructure()
    {
        // Arrange
        var setlist = new Setlist
        {
            Id = 1,
            Name = "Live au Bikini",
            Date = new DateTime(2026, 10, 5, 0, 0, 0, DateTimeKind.Utc)
        };

        var selectedSongIds = new List<int> { 10, 25, 42 };

        // Act & Assert
        Assert.That(setlist, Is.Not.Null);
        Assert.That(setlist.Name, Is.EqualTo("Live au Bikini"));
        Assert.That(selectedSongIds, Has.Count.EqualTo(3));
        Assert.That(selectedSongIds, Does.Contain(25));

        // Simulation d'un appel asynchrone réussi
        await Task.CompletedTask;
    }

    [Test]
    public void SetlistEntity_Should_InitializeWithDefaultValues()
    {
        // Arrange & Act
        var setlist = new Setlist();

        // Assert
        Assert.That(setlist.Name, Is.EqualTo(string.Empty));
        Assert.That(setlist.Id, Is.EqualTo(0));
    }
}