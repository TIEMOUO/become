using Become.ViewModels;

namespace Become.Views;

public partial class GoalSuggestionsPage : ContentPage
{
    private readonly GoalSuggestionsViewModel _vm;

    public GoalSuggestionsPage(GoalSuggestionsViewModel vm)
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