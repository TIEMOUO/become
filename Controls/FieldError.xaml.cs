namespace Become.Controls;

public partial class FieldError : ContentView
{
    public static readonly BindableProperty MessageProperty =
        BindableProperty.Create(nameof(Message), typeof(string), typeof(FieldError), string.Empty,
            propertyChanged: (b, _, n) => ((FieldError)b).IsVisible = !string.IsNullOrEmpty(n as string));

    public FieldError() => InitializeComponent();

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }
}