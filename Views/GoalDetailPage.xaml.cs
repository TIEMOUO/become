using Become.Helpers;
using Become.ViewModels;

namespace Become.Views;

public partial class GoalDetailPage : ContentPage
{
    public GoalDetailPage(GoalDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;   // reçoit l'id via IQueryAttributable
    }

    private async void OnTabSelected(object? sender, string tab)
        => await AppNavigator.OpenTabAsync(tab);
}
