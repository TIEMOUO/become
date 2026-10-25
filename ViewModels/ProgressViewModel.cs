using System.Collections.ObjectModel;
using Become.Domain;
using Become.Services;

namespace Become.ViewModels;

public record DayDot(string Letter, bool IsActive, bool IsToday);
public record GoalProgressRow(string Title, string Domain, double Progress, string StatusText);
public record HistoryEntry(string Title, string GoalTitle, bool IsDone);

public class HistoryGroup
{
    public string DateLabel { get; set; } = "";
    public string Summary { get; set; } = "";
    public List<HistoryEntry> Entries { get; set; } = new();
}

public class ProgressViewModel : BecomeViewModelBase
{
    private readonly IBecomeStore _store;
    private string _totalDoneText = "0", _weekDoneText = "0", _regularityText = "";
    private string _achievedText = "";
    private bool _hasHistory, _hasGoals;

    public ObservableCollection<DayDot> WeekDays { get; } = new();
    public ObservableCollection<GoalProgressRow> GoalRows { get; } = new();
    public ObservableCollection<HistoryGroup> History { get; } = new();

    public string TotalDoneText { get => _totalDoneText; set => Set(ref _totalDoneText, value); }
    public string WeekDoneText { get => _weekDoneText; set => Set(ref _weekDoneText, value); }
    public string RegularityText { get => _regularityText; set => Set(ref _regularityText, value); }
    public string AchievedText { get => _achievedText; set => Set(ref _achievedText, value); }
    public bool HasHistory { get => _hasHistory; set => Set(ref _hasHistory, value); }
    public bool HasGoals { get => _hasGoals; set => Set(ref _hasGoals, value); }

    public ProgressViewModel(IBecomeStore store)
    {
        _store = store;
    }

    public async Task LoadAsync()
    {
        var d = await _store.GetAsync();
        var today = DateTime.Today;
        var letters = new[] { "D", "L", "M", "M", "J", "V", "S" };

        DateTime DoneDay(DailyAction a) => (a.DoneAt ?? a.ScheduledDate).Date;

        var doneActions = d.Actions.Where(a => a.IsDone).ToList();
        var doneDays = doneActions.Select(DoneDay).ToHashSet();

        TotalDoneText = doneActions.Count.ToString();
        WeekDoneText = doneActions.Count(a => DoneDay(a) >= today.AddDays(-6)).ToString();

        WeekDays.Clear();
        var activeDays = 0;
        for (var i = 6; i >= 0; i--)
        {
            var day = today.AddDays(-i);
            var active = doneDays.Contains(day);
            if (active) activeDays++;
            WeekDays.Add(new DayDot(letters[(int)day.DayOfWeek], active, i == 0));
        }
        RegularityText = $"{activeDays} jour(s) actif(s) sur les 7 derniers";

        GoalRows.Clear();
        foreach (var g in d.Goals.Where(g => g.Status != GoalStatus.Archived))
        {
            var progress = BecomeStore.EstimateProgress(d, g);
            var status = g.Status == GoalStatus.Achieved
                ? "Atteint"
                : $"{(int)Math.Round(progress * 100)} % estimé · en cours";
            GoalRows.Add(new GoalProgressRow(g.Title, g.Domain, progress, status));
        }
        HasGoals = GoalRows.Count > 0;

        var achieved = d.Goals.Count(g => g.Status == GoalStatus.Achieved);
        AchievedText = $"Objectifs atteints : {achieved}";

        var goalTitles = d.Goals.ToDictionary(g => g.Id, g => g.Title);
        History.Clear();
        var groups = d.Actions
            .Where(a => a.ScheduledDate.Date < today && a.ScheduledDate.Date >= today.AddDays(-14))
            .GroupBy(a => a.ScheduledDate.Date)
            .OrderByDescending(g => g.Key);

        foreach (var group in groups)
        {
            var entries = group
                .Select(a => new HistoryEntry(a.Title, goalTitles.GetValueOrDefault(a.GoalId, ""), a.IsDone))
                .ToList();

            History.Add(new HistoryGroup
            {
                DateLabel = FrenchDates.Long(group.Key),
                Summary = $"{entries.Count(e => e.IsDone)} sur {entries.Count} réalisée(s)",
                Entries = entries
            });
        }
        HasHistory = History.Count > 0;
    }
}