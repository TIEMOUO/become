namespace Become.Controls;

public partial class BecomeBackButton : ContentView
{
    public BecomeBackButton()
    {
        InitializeComponent();
    }

    private async void OnTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}