using Become.ViewModels;

namespace Become.Views;

public partial class PrioritiesPage : ContentPage
{
    private readonly PrioritiesViewModel _vm;

    public PrioritiesPage(PrioritiesViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }
}