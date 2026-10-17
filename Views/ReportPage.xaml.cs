using Become.Helpers;
using Become.ViewModels;

namespace Become.Views;

public partial class ReportPage : ContentPage
{
    private readonly ReportViewModel _viewModel;

    public ReportPage(ReportViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.Refresh();
    }

    private async void OnTabSelected(object? sender, string tab)
        => await AppNavigator.OpenTabAsync(tab);
}
