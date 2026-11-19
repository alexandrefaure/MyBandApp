using MyBandApp.Models.Entities;
using MyBandApp.Models.Presentation;
using NUnit.Framework;
using System.Collections.ObjectModel;

namespace MyBandApp.Tests;

[TestFixture]
public class SetlistEditPageTests
{
    private List<SelectableSongModel> _allSongsSample = null!;

    [SetUp]
    public void SetUp()
    {
        // Données de test réutilisables
        _allSongsSample = new List<SelectableSongModel>
        {
            new() { Song = new Song { Id = 1, Title = "Get Lucky", Artist = "Daft Punk" }, IsSelected = false },
            new() { Song = new Song { Id = 2, Title = "Superstition", Artist = "Stevie Wonder" }, IsSelected = true },
            new() { Song = new Song { Id = 3, Title = "Africa", Artist = "Toto" }, IsSelected = false },
            new() { Song = new Song { Id = 4, Title = "Sir Duke", Artist = "Stevie Wonder" }, IsSelected = true }
        };
    }

    [Test]
    [TestCase("Daft", 1)]
    [TestCase("Stevie", 2)]
    [TestCase("Toto", 1)]
    [TestCase("Inexistant", 0)]
    public void FilterCatalogue_Should_FilterByTitleOrArtist(string query, int expectedCount)
    {
        // Act
        var filtered = _allSongsSample.Where(x =>
            x.Song.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            x.Song.Artist.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

        // Assert (Syntaxe NUnit Fluent / Classic)
        Assert.That(filtered, Has.Count.EqualTo(expectedCount));
    }

    [Test]
    public void UpdateSelectedSongs_Should_AssignContinuousDisplayIndex()
    {
        // Arrange
        var selectedList = _allSongsSample.Where(x => x.IsSelected).ToList();

        // Act (Simulation de la logique de numérotation)
        for (int i = 0; i < selectedList.Count; i++)
        {
            selectedList[i].DisplayIndex = i + 1;
        }

        // Assert
        Assert.That(selectedList, Has.Count.EqualTo(2));
        Assert.That(selectedList[0].DisplayIndex, Is.EqualTo(1));
        Assert.That(selectedList[0].Song.Title, Is.EqualTo("Superstition"));

        Assert.That(selectedList[1].DisplayIndex, Is.EqualTo(2));
        Assert.That(selectedList[1].Song.Title, Is.EqualTo("Sir Duke"));
    }

    [Test]
    public void ReorderSelectedSongs_Should_UpdateIndicesCorrectly()
    {
        // Arrange
        var selectedSongsCollection = new ObservableCollection<SelectableSongModel>(
            _allSongsSample.Where(x => x.IsSelected)
        );

        // Simulation d'un déplacement (le 2e passe en 1er)
        var itemToMove = selectedSongsCollection[1];
        selectedSongsCollection.Move(1, 0);

        // Act : Recalcul des index d'affichage
        for (int i = 0; i < selectedSongsCollection.Count; i++)
        {
            selectedSongsCollection[i].DisplayIndex = i + 1;
        }

        // Assert
        Assert.That(selectedSongsCollection[0].Song.Title, Is.EqualTo("Sir Duke"));
        Assert.That(selectedSongsCollection[0].DisplayIndex, Is.EqualTo(1));

        Assert.That(selectedSongsCollection[1].Song.Title, Is.EqualTo("Superstition"));
        Assert.That(selectedSongsCollection[1].DisplayIndex, Is.EqualTo(2));
    }
}