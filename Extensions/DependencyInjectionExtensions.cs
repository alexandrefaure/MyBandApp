using System.Reflection;

namespace MyBandApp.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection RegisterAppByConvention(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        foreach (var type in assembly.GetTypes())
        {
            // On s'assure que c'est une classe concrète qui appartient au namespace du projet
            if (!type.IsClass || type.IsAbstract || type.Namespace == null || !type.Namespace.StartsWith("MyBandApp"))
                continue;

            string name = type.Name;

            if (name.EndsWith("Service"))
            {
                // Les services métier/données en Singleton
                services.AddSingleton(type);
            }
            else if (name.EndsWith("ViewModel"))
            {
                // Les ViewModels en Transient
                services.AddTransient(type);
            }
            else if (name.EndsWith("Page"))
            {
                // Les Vues / Pages / Popups en Transient
                services.AddTransient(type);
            }
        }

        return services;
    }
}