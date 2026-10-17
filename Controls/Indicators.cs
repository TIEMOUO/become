using Become.Helpers;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace Become.Controls;

// =====================================================================
// Anneau de progression (GraphicsView : un seul tracé vectoriel, sans image)
// =====================================================================
public class ProgressRing : ContentView
{
    public static readonly BindableProperty ProgressProperty =
        BindableProperty.Create(nameof(Progress), typeof(double), typeof(ProgressRing), 0.0,
            propertyChanged: (b, _, _) => ((ProgressRing)b).Refresh());

    public static readonly BindableProperty CaptionProperty =
        BindableProperty.Create(nameof(Caption), typeof(string), typeof(ProgressRing), "",
            propertyChanged: (b, _, n) => ((ProgressRing)b)._caption.Text = (string)n);

    private readonly RingDrawable _drawable = new();
    private readonly GraphicsView _view;
    private readonly Label _percent;
    private readonly Label _caption;

    public ProgressRing()
    {
        _view = new GraphicsView { Drawable = _drawable };

        _percent = new Label
        {
            FontFamily = "InterBold",
            FontSize = 28,
            TextColor = AppColors.TextPrimary,
            HorizontalTextAlignment = TextAlignment.Center
        };

        _caption = new Label
        {
            FontFamily = "InterRegular",
            FontSize = 14,
            TextColor = AppColors.TextSecondary,
            HorizontalTextAlignment = TextAlignment.Center
        };

        var center = new VerticalStackLayout
        {
            Spacing = 0,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            Children = { _percent, _caption }
        };

        Content = new Grid { Children = { _view, center } };
        Refresh();
    }

    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    public string Caption
    {
        get => (string)GetValue(CaptionProperty);
        set => SetValue(CaptionProperty, value);
    }

    private void Refresh()
    {
        var p = Math.Clamp(Progress, 0, 1);
        _drawable.Progress = p;
        _percent.Text = $"{Math.Round(p * 100, MidpointRounding.AwayFromZero)}%";
        SemanticProperties.SetDescription(this, $"Avancement {_percent.Text}");
        _view.Invalidate();
    }

    private sealed class RingDrawable : IDrawable
    {
        public double Progress { get; set; }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            const float thickness = 10f;
            float x = dirtyRect.X + thickness / 2;
            float y = dirtyRect.Y + thickness / 2;
            float w = dirtyRect.Width - thickness;
            float h = dirtyRect.Height - thickness;

            canvas.StrokeSize = thickness;
            canvas.StrokeLineCap = LineCap.Round;

            // Piste
            canvas.StrokeColor = AppColors.Stroke;
            canvas.DrawEllipse(x, y, w, h);

            if (Progress <= 0) return;

            // Progression (départ à 12 h, sens horaire)
            canvas.StrokeColor = AppColors.Primary;
            if (Progress >= 1)
            {
                canvas.DrawEllipse(x, y, w, h);
                return;
            }

            float endAngle = 90f - (float)(Progress * 360.0);
            canvas.DrawArc(x, y, w, h, 90f, endAngle, true, false);
        }
    }
}

// =====================================================================
// Barre de progression linéaire (arrondie, sans dépendance de plateforme)
// =====================================================================
public class LinearProgress : ContentView
{
    public static readonly BindableProperty ProgressProperty =
        BindableProperty.Create(nameof(Progress), typeof(double), typeof(LinearProgress), 0.0,
            propertyChanged: (b, _, _) => ((LinearProgress)b).Refresh());

    private readonly Grid _grid;

    public LinearProgress()
    {
        _grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(0, GridUnitType.Star)),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star))
            }
        };
        _grid.Add(new BoxView { Color = AppColors.Primary }, 0, 0);

        Content = new Border
        {
            HeightRequest = 8,
            StrokeThickness = 0,
            BackgroundColor = AppColors.Stroke,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(4) },
            Content = _grid
        };
        Refresh();
    }

    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    private void Refresh()
    {
        var p = Math.Clamp(Progress, 0, 1);
        _grid.ColumnDefinitions[0].Width = new GridLength(p, GridUnitType.Star);
        _grid.ColumnDefinitions[1].Width = new GridLength(1 - p, GridUnitType.Star);
    }
}

// =====================================================================
// Barre de navigation inférieure : Accueil / Objectifs / Suivi / Bilan
// =====================================================================
public class BottomNavBar : ContentView
{
    public static readonly BindableProperty ActiveTabProperty =
        BindableProperty.Create(nameof(ActiveTab), typeof(string), typeof(BottomNavBar), "home",
            propertyChanged: (b, _, _) => ((BottomNavBar)b).Build());

    private static readonly (string Key, string Label, string Icon)[] Tabs =
    {
        ("home",     "Accueil",   Icons.Home),
        ("goals",    "Objectifs", Icons.Goals),
        ("tracking", "Suivi",     Icons.Tracking),
        ("report",   "Bilan",     Icons.Report),
    };

    public event EventHandler<string>? TabSelected;

    public BottomNavBar() => Build();

    public string ActiveTab
    {
        get => (string)GetValue(ActiveTabProperty);
        set => SetValue(ActiveTabProperty, value);
    }

    private void Build()
    {
        var grid = new Grid { BackgroundColor = AppColors.Surface, Padding = new Thickness(8, 8, 8, 4) };

        for (int i = 0; i < Tabs.Length; i++)
        {
            var (key, label, icon) = Tabs[i];
            var color = key == ActiveTab ? AppColors.Primary : AppColors.TextSecondary;

            var item = new VerticalStackLayout
            {
                Spacing = 2,
                MinimumHeightRequest = 48,           // zone tactile >= 48
                HorizontalOptions = LayoutOptions.Fill,
                Children =
                {
                    new Label
                    {
                        Text = icon, FontSize = 22, TextColor = color,
                        HorizontalTextAlignment = TextAlignment.Center
                    },
                    new Label
                    {
                        Text = label, FontSize = 13,
                        FontFamily = key == ActiveTab ? "InterSemibold" : "InterRegular",
                        TextColor = color,
                        HorizontalTextAlignment = TextAlignment.Center
                    }
                }
            };

            SemanticProperties.SetDescription(item, key == ActiveTab ? $"{label}, onglet actif" : label);

            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => TabSelected?.Invoke(this, key);
            item.GestureRecognizers.Add(tap);

            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            grid.Add(item, i, 0);
        }

        Content = new VerticalStackLayout
        {
            Spacing = 0,
            Children =
            {
                new BoxView { HeightRequest = 1, Color = AppColors.Stroke },
                grid
            }
        };
    }
}
