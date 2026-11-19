using MyBandApp.Extensions;
using MyBandApp.Services;
using ReactiveUI.Builder;

namespace MyBandApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseReactiveUI(reactiveUi => reactiveUi.WithMaui())
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // 1. Enregistrement de Supabase (client unique)
        builder.Services.AddSingleton(provider =>
            new Supabase.Client(
                "https://ttujmufkxsoxpfvqkabl.supabase.co",
                "sb_publishable_psNwSZtY13UbosiZBNkJhA_182xuVdr",
                new Supabase.SupabaseOptions
                {
                    AutoConnectRealtime = false
                }
            ));

        // 2. Enregistrement automatique de tous les Services, ViewModels et Pages
        builder.Services.RegisterAppByConvention();
        builder.Services.AddSingleton<IUserDialogService>(provider =>
            provider.GetRequiredService<MauiUserDialogService>());
        builder.Services.AddSingleton<IAppNavigationService>(provider =>
            provider.GetRequiredService<MauiNavigationService>());

        return builder.Build();
    }
}