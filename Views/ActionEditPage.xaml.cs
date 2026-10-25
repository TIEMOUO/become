using Become.ViewModels;

namespace Become.Views;

public partial class ActionEditPage : ContentPage
{
    public ActionEditPage(ActionEditViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}