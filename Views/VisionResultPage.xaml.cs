using Become.ViewModels;

namespace Become.Views;

public partial class VisionResultPage : ContentPage
{
    private readonly VisionResultViewModel _vm;

    public VisionResultPage(VisionResultViewModel vm)
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