namespace Become.Controls;

public partial class AuthHeader : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(AuthHeader), string.Empty);

    public static readonly BindableProperty SubtitleProperty =
        BindableProperty.Create(nameof(Subtitle), typeof(string), typeof(AuthHeader), string.Empty,
            propertyChanged: (b, _, _) => ((AuthHeader)b).ApplySubtitle());

    public AuthHeader()
    {
        InitializeComponent();
        ApplySubtitle();
    }

    public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public string Subtitle { get => (string)GetValue(SubtitleProperty); set => SetValue(SubtitleProperty, value); }

    private void ApplySubtitle()
    {
        if (SubtitleLabel is null) return;
        SubtitleLabel.IsVisible = !string.IsNullOrEmpty(Subtitle);
    }
}