using Become.ViewModels;

namespace Become.Views;

public partial class GoalEditPage : ContentPage
{
    public GoalEditPage(GoalEditViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}