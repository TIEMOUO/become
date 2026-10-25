using Become.Views;

namespace Become;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        Routing.RegisterRoute(nameof(VisionQuestionnairePage), typeof(VisionQuestionnairePage));
        Routing.RegisterRoute(nameof(VisionResultPage), typeof(VisionResultPage));
        Routing.RegisterRoute(nameof(PrioritiesPage), typeof(PrioritiesPage));
        Routing.RegisterRoute(nameof(GoalSuggestionsPage), typeof(GoalSuggestionsPage));
        Routing.RegisterRoute(nameof(GoalEditPage), typeof(GoalEditPage));
        Routing.RegisterRoute(nameof(ActionEditPage), typeof(ActionEditPage));
    }
}