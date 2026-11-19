using MyBandApp.Models.Entities;
using MyBandApp.Services;
using NUnit.Framework;

namespace MyBandApp.Tests.Services;

[TestFixture]
public class SetlistServiceTests
{
    [Test]
    public void Constructor_Should_CreateServiceInstance()
    {
        var service = new SetlistService(null!);

        Assert.That(service, Is.Not.Null);
    }

    [Test]
    public void Service_Should_ExposeExpectedOperations()
    {
        var methodNames = typeof(SetlistService).GetMethods().Select(x => x.Name).ToHashSet();

        Assert.That(methodNames, Does.Contain(nameof(SetlistService.GetSetlistsWithSongsAsync)));
        Assert.That(methodNames, Does.Contain(nameof(SetlistService.GetSongsForSetlistAsync)));
        Assert.That(methodNames, Does.Contain(nameof(SetlistService.SaveSetlistAsync)));
        Assert.That(methodNames, Does.Contain(nameof(SetlistService.DeleteSetlistAsync)));
    }

    [Test]
    public void SetlistEntity_Should_InitializeWithDefaultValues()
    {
        var setlist = new Setlist();

        Assert.That(setlist.Id, Is.EqualTo(0));
        Assert.That(setlist.Name, Is.EqualTo(string.Empty));
        Assert.That(setlist.Date, Is.EqualTo(DateTime.Today).Within(TimeSpan.FromSeconds(1)));
    }
}
