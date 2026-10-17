using Become.ViewModels;

namespace Become.Views;

public partial class RegisterPage : ContentPage
{
    // TODO : remplace par les vraies pages de ton application / site
    private const string TermsUrl = "https://example.com/conditions";
    private const string PrivacyUrl = "https://example.com/confidentialite";

    public RegisterPage(RegisterViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnTermsTapped(object? sender, TappedEventArgs e) => await OpenAsync(TermsUrl);

    private async void OnPrivacyTapped(object? sender, TappedEventArgs e) => await OpenAsync(PrivacyUrl);

    private static async Task OpenAsync(string url)
    {
        try { await Launcher.Default.OpenAsync(url); }
        catch { /* pas de navigateur disponible : on ignore */ }
    }
}
