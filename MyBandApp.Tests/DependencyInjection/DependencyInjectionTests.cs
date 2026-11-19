using Microsoft.Extensions.DependencyInjection;
using MyBandApp.Services;
using NUnit.Framework;

namespace MyBandApp.Tests.DependencyInjection;

[TestFixture]
public class DependencyInjectionTests
{
    private ServiceProvider _serviceProvider = null!;

    [SetUp]
    public void SetUp()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new SetlistService(null!));
        services.AddSingleton(new SongService(null!));

        _serviceProvider = services.BuildServiceProvider();
    }

    [TearDown]
    public void TearDown()
    {
        _serviceProvider.Dispose();
    }

    [Test]
    public void ServiceProvider_Should_Resolve_SetlistService()
    {
        var service = _serviceProvider.GetService<SetlistService>();

        Assert.That(service, Is.Not.Null, "Le conteneur DI n'a pas réussi à instancier le SetlistService.");
    }

    [Test]
    public void ServiceProvider_Should_Resolve_SongService()
    {
        var service = _serviceProvider.GetService<SongService>();

        Assert.That(service, Is.Not.Null, "Le conteneur DI n'a pas réussi à instancier le SongService.");
    }
}
