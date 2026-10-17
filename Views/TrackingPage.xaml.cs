using Become.Helpers;
using Become.ViewModels;

namespace Become.Views;

public partial class TrackingPage : ContentPage
{
    private readonly TrackingViewModel _viewModel;

    public TrackingPage(TrackingViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.Refresh();
    }

    private void OnTaskTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject { BindingContext: TaskItem item })
            _viewModel.Toggle(item);
    }

    private async void OnTabSelected(object? sender, string tab)
        => await AppNavigator.OpenTabAsync(tab);
}
