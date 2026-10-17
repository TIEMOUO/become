using System.Collections.ObjectModel;
using System.Windows.Input;
using Become.Helpers;
using Become.Models;
using Become.Services;

namespace Become.ViewModels;

public record WeekBar(string Range, string Counts, string PercentText, double BarHeight, bool IsHighlight);

public record MonthGoalItem(string Title, string Detail, string StatusText, bool IsAchieved, bool IsFinished);

public class ReportViewModel : BaseViewModel
{
    private readonly IGoalStore _store;

    private DateTime _month = new(DateTime.Today.Year, DateTime.Today.Month, 1);

    private string _regularityText = "—";
    private string _regularityDetail = "";
    private string _achievedText = "—";
    private string _achievedDetail = "";
    private string _chartTitle = "";
    private string _insight = "";
    private bool _hasData;

    public ReportViewModel(IGoalStore store)
    {
        _store = store;
        PreviousCommand = new Command(() => { _month = _month.AddMonths(-1); Refresh(); });
        NextCommand = new Command(() => { if (CanGoNext) { _month = _month.AddMonths(1); Refresh(); } });
        CreateGoalCommand = new Command(async () => await AppNavigator.OpenCreateGoalAsync());
    }

    public ObservableCollection<WeekBar> Weeks { get; } = new();
    public ObservableCollection<MonthGoalItem> MonthGoals { get; } = new();

    public string MonthTitle => Fmt.Month(_month);
    public bool CanGoNext => _month < new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
    public double NextOpacity => CanGoNext ? 1.0 : 0.3;

    public string RegularityText { get => _regularityText; private set => SetProperty(ref _regularityText, value); }
    public string RegularityDetail { get => _regularityDetail; private set => SetProperty(ref _regularityDetail, value); }
    public string AchievedText { get => _achievedText; private set => SetProperty(ref _achievedText, value); }
    public string AchievedDetail { get => _achievedDetail; private set => SetProperty(ref _achievedDetail, value); }
    public string ChartTitle { get => _chartTitle; private set => SetProperty(ref _chartTitle, value); }
    public string ChartCaption => $"Actions réalisées / prévues • {_month.ToString("MMMM", Fmt.Fr)}";
    public string Insight { get => _insight; private set => SetProperty(ref _insight, value); }

    public bool HasData
    {
        get => _hasData;
        private set { if (SetProperty(ref _hasData, value)) OnPropertyChanged(nameof(HasNoData)); }
    }
    public bool HasNoData => !HasData;
    public string EmptyText => $"Pas encore de données pour {_month.ToString("MMMM yyyy", Fmt.Fr)}. Validez vos premières séances pour voir votre progression ici.";
    public bool HasMonthGoals => MonthGoals.Count > 0;

    public ICommand PreviousCommand { get; }
    public ICommand NextCommand { get; }
    public ICommand CreateGoalCommand { get; }

    // ------------------------------------------------------------------
    // Calculs : tout est local, aucun chargement réseau
    // ------------------------------------------------------------------
    private (int Planned, int Done) Count(DateTime rangeStart, DateTime rangeEnd)
    {
        var today = DateTime.Today;
        int planned = 0, done = 0;

        foreach (var g in _store.Goals)
        {
            var doneInRange = g.Sessions.Count(s => s.Date.Date >= rangeStart && s.Date.Date <= rangeEnd);
            done += doneInRange;

            if (g.Frequency == Frequency.Free)
            {
                planned += doneInRange;   // pas de planning imposé : on ne pénalise pas
                continue;
            }

            var first = rangeStart > g.StartDate.Date ? rangeStart : g.StartDate.Date;
            var end = new[] { rangeEnd, g.TargetDate.Date, today }.Min();
            for (var d = first; d <= end; d = d.AddDays(1))
                if (g.IsScheduledOn(d)) planned++;
        }
        return (planned, done);
    }

    private static int Percent(int planned, int done)
        => planned == 0 ? (done > 0 ? 100 : 0) : Math.Min(100, (int)Math.Round(done * 100.0 / planned));

    public void Refresh()
    {
        var monthStart = _month;
        var monthEnd = _month.AddMonths(1).AddDays(-1);

        // --- Régularité du mois ---
        var (planned, done) = Count(monthStart, monthEnd);
        HasData = planned > 0 || done > 0;
        RegularityText = HasData ? $"{Percent(planned, done)}%" : "—";
        RegularityDetail = HasData ? $"{done} / {planned} actions prévues" : "Aucune action prévue";

        // --- Évolution par semaine ---
        Weeks.Clear();
        var edges = new[] { (1, 7), (8, 14), (15, 21), (22, monthEnd.Day) };
        var stats = edges.Select(e =>
        {
            var weekStart = new DateTime(_month.Year, _month.Month, e.Item1);
            var weekEnd = new DateTime(_month.Year, _month.Month, e.Item2);
            var (p, d) = Count(weekStart, weekEnd);
            return (Label: $"{e.Item1}–{e.Item2}", Planned: p, Done: d);
        }).ToList();

        var lastWithData = stats.FindLastIndex(s => s.Planned > 0 || s.Done > 0);
        for (var i = 0; i < stats.Count; i++)
        {
            var s = stats[i];
            var hasValue = s.Planned > 0 || s.Done > 0;
            var pct = Percent(s.Planned, s.Done);
            Weeks.Add(new WeekBar(
                s.Label,
                hasValue ? $"{s.Done}/{s.Planned}" : "—",
                hasValue ? $"{pct}%" : "—",
                hasValue ? 12 + pct * 1.0 : 6,
                i == lastWithData));
        }

        var values = stats.Where(s => s.Planned > 0 || s.Done > 0).Select(s => Percent(s.Planned, s.Done)).ToList();
        ChartTitle = values.Count >= 2 && values[^1] >= values[0]
            ? "Une régularité qui grandit"
            : "Votre régularité, semaine par semaine";

        // --- Objectifs du mois ---
        MonthGoals.Clear();
        var inMonth = _store.Goals
            .Where(g => g.StartDate.Date <= monthEnd && g.TargetDate.Date >= monthStart)
            .OrderBy(g => g.TargetDate)
            .ToList();
        foreach (var g in inMonth)
            MonthGoals.Add(new MonthGoalItem(g.Title, g.ProgressText, g.StatusText, g.IsAchieved, g.IsFinished));

        var due = _store.Goals.Where(g => g.TargetDate.Date >= monthStart && g.TargetDate.Date <= monthEnd).ToList();
        AchievedText = due.Count > 0 ? $"{due.Count(g => g.IsAchieved)} / {due.Count}" : "—";
        AchievedDetail = due.Count > 0 ? "objectifs arrivés à échéance" : "aucun objectif à échéance";

        // --- Message ---
        var missed = due.FirstOrDefault(g => g.IsFinished && !g.IsAchieved);
        Insight = missed is not null
            ? $"Pour « {missed.Title} », essayez des séances plus courtes : une petite séance vaut mieux qu'aucune."
            : HasData && Percent(planned, done) >= 80
                ? "Belle régularité ce mois-ci. Continuez à votre rythme."
                : "Chaque séance compte. Reprenez dès aujourd'hui, sans pression.";

        OnPropertyChanged(string.Empty);
    }
}
