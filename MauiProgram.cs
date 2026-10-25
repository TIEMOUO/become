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
                fonts.AddFont("MaterialIconsRound-Subset.otf", "MaterialIcons");   // icônes vectorielles
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
        builder.Services.AddTransient<ReportViewModel>();

        // Pages
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<ForgotPasswordPage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<GoalsPage>();
        builder.Services.AddTransient<CreateGoalPage>();
        builder.Services.AddTransient<GoalDetailPage>();
        builder.Services.AddTransient<TrackingPage>();
        builder.Services.AddTransient<ReportPage>();

        builder.Services.AddSingleton<Become.Services.IAccountService, Become.Services.LocalAccountService>();

        builder.Services.AddTransient<Become.ViewModels.LoginViewModel>();
        builder.Services.AddTransient<Become.ViewModels.RegisterViewModel>();

        builder.Services.AddTransient<Become.Views.LoginPage>();
        builder.Services.AddTransient<Become.Views.RegisterPage>();

        builder.Services.AddSingleton<IBecomeStore, BecomeStore>();

        builder.Services.AddTransient<VisionQuestionnaireViewModel>();
        builder.Services.AddTransient<VisionResultViewModel>();
        builder.Services.AddTransient<PrioritiesViewModel>();
        builder.Services.AddTransient<GoalSuggestionsViewModel>();
        builder.Services.AddTransient<GoalEditViewModel>();
        builder.Services.AddTransient<ActionEditViewModel>();
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<GoalsViewModel>();
        builder.Services.AddTransient<ProgressViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();

        builder.Services.AddTransient<WelcomePage>();
        builder.Services.AddTransient<VisionQuestionnairePage>();
        builder.Services.AddTransient<VisionResultPage>();
        builder.Services.AddTransient<PrioritiesPage>();
        builder.Services.AddTransient<GoalSuggestionsPage>();
        builder.Services.AddTransient<GoalEditPage>();
        builder.Services.AddTransient<ActionEditPage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<GoalsPage>();
        builder.Services.AddTransient<ProgressPage>();
        builder.Services.AddTransient<ProfilePage>();
        return builder.Build();

    }
}
