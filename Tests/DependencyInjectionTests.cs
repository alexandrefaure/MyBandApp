using MyBandApp.Services;
using NUnit.Framework;

namespace MyBandApp.Tests;

[TestFixture]
public class DependencyInjectionTests
{
    private ServiceProvider _serviceProvider = null!;

    [SetUp]
    public void SetUp()
    {
        var services = new ServiceCollection();

        // Enregistrement des services de l'application (à adapter selon ta configuration DI réelle)
        services.AddTransient<SetlistService>();
        // Si tu utilises un client Supabase ou d'autres services, tu peux les enregistrer ici pour les tester

        _serviceProvider = services.BuildServiceProvider();
    }

    [TearDown]
    public void TearDown()
    {
        _serviceProvider?.Dispose();
    }

    [Test]
    public void ServiceProvider_Should_Resolve_SetlistService()
    {
        // Act
        var service = _serviceProvider.GetService<SetlistService>();

        // Assert
        Assert.That(service, Is.Not.Null, "Le conteneur DI n'a pas réussi à instancier le SetlistService.");
    }
}