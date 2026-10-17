using Become.Services;
using Become.ViewModels;
using Become.Views;

namespace Become;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Inter-Regular.ttf", "InterRegular");
                fonts.AddFont("Inter-SemiBold.ttf", "InterSemibold");
                fonts.AddFont("Inter-Bold.ttf", "InterBold");
            });

        // Services
        builder.Services.AddSingleton<IGoalStore, JsonGoalStore>();
        builder.Services.AddSingleton<IAuthService, FakeAuthService>();

        // ViewModels
        builder.Services.AddTransient<LoginViewModel>();

        // Pages
        builder.Services.AddTransient<LoginPage>();

        return builder.Build();
    }
}
