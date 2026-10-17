namespace Become.Helpers;

public static class AppNavigator
{
    // Routes racine (onglets) et routes empilées
    public const string Login = "//login";
    public const string Home = "//home";
    public const string Goals = "//goals";
    public const string Tracking = "//tracking";
    public const string Report = "//report";

    public const string RegisterRoute = "register";
    public const string ForgotRoute = "forgot-password";
    public const string CreateRoute = "create-goal";
    public const string DetailRoute = "goal-detail";

    public static Task GoHomeAsync() => Shell.Current.GoToAsync(Home);
    public static Task GoToLoginAsync() => Shell.Current.GoToAsync(Login);
    public static Task OpenRegisterAsync() => Shell.Current.GoToAsync(RegisterRoute);
    public static Task OpenForgotPasswordAsync() => Shell.Current.GoToAsync(ForgotRoute);
    public static Task OpenCreateGoalAsync() => Shell.Current.GoToAsync(CreateRoute);
    public static Task OpenGoalAsync(string id) => Shell.Current.GoToAsync($"{DetailRoute}?id={id}");
    public static Task BackAsync() => Shell.Current.GoToAsync("..");

    public static Task OpenTabAsync(string tab) => Shell.Current.GoToAsync(tab switch
    {
        "goals" => Goals,
        "tracking" => Tracking,
        "report" => Report,
        _ => Home
    });
}
