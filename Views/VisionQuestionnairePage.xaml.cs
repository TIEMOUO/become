using Become.ViewModels;

namespace Become.Views;

public partial class VisionQuestionnairePage : ContentPage
{
    private readonly VisionQuestionnaireViewModel _vm;

    public VisionQuestionnairePage(VisionQuestionnaireViewModel vm)
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