using Become.Helpers;
using Become.ViewModels;

namespace Become.Views;

public partial class CreateGoalPage : ContentPage
{
    private readonly CreateGoalViewModel _viewModel;

    public CreateGoalPage(CreateGoalViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    private void OnDayTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject { BindingContext: DayChip chip })
            _viewModel.ToggleDay(chip);
    }

    private async void OnTabSelected(object? sender, string tab)
        => await AppNavigator.OpenTabAsync(tab);

    // Bouton retour système : même confirmation que « Annuler »
    protected override bool OnBackButtonPressed()
    {
        _ = _viewModel.CancelAsync();
        return true;
    }
}
