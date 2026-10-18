using Become.Helpers;

namespace Become.Controls;

public partial class FormField : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(FormField), string.Empty,
            propertyChanged: (b, _, _) => ((FormField)b).ApplyHeader());

    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(string), typeof(FormField), string.Empty,
            propertyChanged: (b, _, _) => ((FormField)b).ApplyHeader());

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(FormField), string.Empty);

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(FormField), string.Empty,
            BindingMode.TwoWay);

    public static readonly BindableProperty IsPasswordProperty =
        BindableProperty.Create(nameof(IsPassword), typeof(bool), typeof(FormField), false,
            propertyChanged: (b, _, _) => ((FormField)b).ApplyMask());

    public static readonly BindableProperty ErrorMessageProperty =
        BindableProperty.Create(nameof(ErrorMessage), typeof(string), typeof(FormField), string.Empty,
            propertyChanged: (b, _, _) => ((FormField)b).ApplyState());

    public static readonly BindableProperty InputKeyboardProperty =
        BindableProperty.Create(nameof(InputKeyboard), typeof(Keyboard), typeof(FormField), Keyboard.Default);

    private bool _revealed;
    private bool _focused;

    public FormField()
    {
        InitializeComponent();
        ApplyHeader();
        ApplyMask();
        ApplyState();
    }

    public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public string Icon { get => (string)GetValue(IconProperty); set => SetValue(IconProperty, value); }
    public string Placeholder { get => (string)GetValue(PlaceholderProperty); set => SetValue(PlaceholderProperty, value); }
    public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
    public bool IsPassword { get => (bool)GetValue(IsPasswordProperty); set => SetValue(IsPasswordProperty, value); }
    public string ErrorMessage { get => (string)GetValue(ErrorMessageProperty); set => SetValue(ErrorMessageProperty, value); }
    public Keyboard InputKeyboard { get => (Keyboard)GetValue(InputKeyboardProperty); set => SetValue(InputKeyboardProperty, value); }

    private void ApplyHeader()
    {
        if (Input is null) return;
        TitleLabel.IsVisible = !string.IsNullOrEmpty(Title);
        IconLabel.IsVisible = !string.IsNullOrEmpty(Icon);
    }

    private void ApplyMask()
    {
        if (Input is null) return;
        Input.IsPassword = IsPassword && !_revealed;
        RevealLabel.IsVisible = IsPassword;
        RevealLabel.Text = _revealed ? AppIcons.VisibilityOff : AppIcons.Visibility;
        SemanticProperties.SetDescription(RevealLabel,
            _revealed ? "Masquer le mot de passe" : "Afficher le mot de passe");
    }

    private void ApplyState()
    {
        if (FieldFrame is null) return;

        var hasError = !string.IsNullOrEmpty(ErrorMessage);

        FieldFrame.Stroke = hasError ? AppColors.Error
                          : _focused ? AppColors.Primary
                          : AppColors.Stroke;

        IconLabel.TextColor = hasError ? AppColors.Error
                            : _focused ? AppColors.Primary
                            : AppColors.TextMuted;
    }

    private void OnFocused(object? sender, FocusEventArgs e)
    {
        _focused = true;
        ApplyState();
    }

    private void OnUnfocused(object? sender, FocusEventArgs e)
    {
        _focused = false;
        ApplyState();
    }

    private void OnRevealTapped(object? sender, TappedEventArgs e)
    {
        _revealed = !_revealed;
        ApplyMask();
    }
}