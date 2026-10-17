using Become.Helpers;
using Become.Models;
using Become.ViewModels;

namespace Become.Views;

public partial class GoalsPage : ContentPage
{
    private readonly GoalsViewModel _viewModel;

    public GoalsPage(GoalsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.Refresh();
    }

    private void OnGoalTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject { BindingContext: Goal goal })
            _viewModel.OpenGoalCommand.Execute(goal);
    }

    private async void OnTabSelected(object? sender, string tab)
        => await AppNavigator.OpenTabAsync(tab);
}
