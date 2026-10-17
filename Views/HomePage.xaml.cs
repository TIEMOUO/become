using Become.Helpers;
using Become.Models;
using Become.ViewModels;

namespace Become.Views;

public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _viewModel;

    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();   // rafraîchit à chaque retour sur l'écran
    }

    private void OnGoalTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject { BindingContext: Goal goal })
            _viewModel.OpenGoalCommand.Execute(goal);
    }

    private async void OnTabSelected(object? sender, string tab)
        => await AppNavigator.OpenTabAsync(tab);
}
