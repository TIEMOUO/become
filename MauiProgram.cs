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
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<ForgotPasswordViewModel>();
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<GoalsViewModel>();
        builder.Services.AddTransient<CreateGoalViewModel>();
        builder.Services.AddTransient<GoalDetailViewModel>();
        builder.Services.AddTransient<TrackingViewModel>();

        // Pages
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<ForgotPasswordPage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<GoalsPage>();
        builder.Services.AddTransient<CreateGoalPage>();
        builder.Services.AddTransient<GoalDetailPage>();
        builder.Services.AddTransient<TrackingPage>();

        return builder.Build();
    }
}
