using Become.Helpers;

namespace Become.Controls;

public enum BannerKind { Error, Success, Info }

public partial class InlineBanner : ContentView
{
    public static readonly BindableProperty MessageProperty =
        BindableProperty.Create(nameof(Message), typeof(string), typeof(InlineBanner), string.Empty);

    public static readonly BindableProperty KindProperty =
        BindableProperty.Create(nameof(Kind), typeof(BannerKind), typeof(InlineBanner), BannerKind.Info,
            propertyChanged: (b, _, _) => ((InlineBanner)b).ApplyKind());

    public InlineBanner()
    {
        InitializeComponent();
        ApplyKind();
    }

    public string Message { get => (string)GetValue(MessageProperty); set => SetValue(MessageProperty, value); }
    public BannerKind Kind { get => (BannerKind)GetValue(KindProperty); set => SetValue(KindProperty, value); }

    private void ApplyKind()
    {
        if (IconLabel is null) return;

        var (icon, color, tint) = Kind switch
        {
            BannerKind.Error => (AppIcons.Error, AppColors.Error, AppColors.ErrorTint),
            BannerKind.Success => (AppIcons.CheckCircle, AppColors.Success, AppColors.SuccessTint),
            _ => (AppIcons.Info, AppColors.Primary, AppColors.PrimaryTint)
        };

        IconLabel.Text = icon;
        IconLabel.TextColor = color;
        BannerFrame.BackgroundColor = tint;
    }
}