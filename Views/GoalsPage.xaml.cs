using Become.ViewModels;

namespace Become.Views;

public partial class GoalsPage : ContentPage
{
    private readonly GoalsViewModel _vm;

    public GoalsPage(GoalsViewModel vm)
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