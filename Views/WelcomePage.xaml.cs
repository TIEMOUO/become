using Become.Services;
namespace Become.Views;

public partial class WelcomePage : ContentPage
{
    private bool _animating;

    public WelcomePage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _animating = true;
        var accounts = IPlatformApplication.Current!.Services.GetService<IAccountService>();
        if (accounts is not null && await accounts.IsAuthenticatedAsync())
        {
            await OnboardingRouter.GoNextAsync();
            return;
        }

        HeroZone.Opacity = 0;
        HeroZone.Scale = 0.85;
        TextBlock.Opacity = 0;
        TextBlock.TranslationY = 24;

        await Task.WhenAll(
            HeroZone.FadeTo(1, 700, Easing.CubicOut),
            HeroZone.ScaleTo(1, 700, Easing.CubicOut));

        await Task.WhenAll(
            TextBlock.FadeTo(1, 500, Easing.CubicOut),
            TextBlock.TranslateTo(0, 0, 500, Easing.CubicOut));

        // Halo qui « respire » en continu
        while (_animating)
        {
            await HeroGlow.ScaleTo(1.12, 1800, Easing.SinInOut);
            await HeroGlow.ScaleTo(1.0, 1800, Easing.SinInOut);
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _animating = false;
    }

    private async void OnStartClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(RegisterPage));
    }

    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(LoginPage));
    }
}