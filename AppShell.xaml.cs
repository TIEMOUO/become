using Become.Helpers;
using Become.Views;

namespace Become;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Pages empilées (le bouton retour système est géré par Shell)
        Routing.RegisterRoute(AppNavigator.RegisterRoute, typeof(RegisterPage));
        Routing.RegisterRoute(AppNavigator.ForgotRoute, typeof(ForgotPasswordPage));
        Routing.RegisterRoute(AppNavigator.CreateRoute, typeof(CreateGoalPage));
    }
}
