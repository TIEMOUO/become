using System.Collections.ObjectModel;
using System.Windows.Input;
using Become.Domain;
using Become.Services;

namespace Become.ViewModels;

public class HomeViewModel : BecomeViewModelBase
{
    private readonly IBecomeStore _store;
    private readonly IAccountService _accounts;

    private string _greeting = "Bonjour", _dateText = "", _visionText = "", _todayText = "";
    private double _todayProgress;
    private bool _hasGoals, _hasNoActions;

    public ObservableCollection<GoalCardRow> Goals { get; } = new();
    public ObservableCollection<TodayActionRow> TodayActions { get; } = new();

    public string Greeting { get => _greeting; set => Set(ref _greeting, value); }
    public string DateText { get => _dateText; set => Set(ref _dateText, value); }
    public string VisionText { get => _visionText; set => Set(ref _visionText, value); }
    public string TodayText { get => _todayText; set => Set(ref _todayText, value); }
    public double TodayProgress { get => _todayProgress; set => Set(ref _todayProgress, value); }

    public bool HasGoals
    {
        get => _hasGoals;
        set { if (Set(ref _hasGoals, value)) OnPropertyChanged(nameof(HasNoGoals)); }
    }
    public bool HasNoGoals => !HasGoals;
    public bool HasNoActions { get => _hasNoActions; set => Set(ref _hasNoActions, value); }

    public ICommand EditVisionCommand { get; }
    public ICommand GoToGoalsCommand { get; }

    public HomeViewModel(IBecomeStore store, IAccountService accounts)
    {
        _store = store;
        _accounts = accounts;
        EditVisionCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(Views.VisionResultPage)));
        GoToGoalsCommand = new Command(async () => await Shell.Current.GoToAsync("//MainTabs/goals"));
    }

    public async Task LoadAsync()
    {
        await _store.EnsureTodayActionsAsync();
        var d = await _store.GetAsync();
        var user = await _accounts.GetCurrentUserAsync();

        var first = user?.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        Greeting = string.IsNullOrEmpty(first) ? "Bonjour" : $"Bonjour, {first}";
        DateText = FrenchDates.Long(DateTime.Today);
        VisionText = d.Vision?.VisionText ?? "";

        var activeGoals = d.Goals.Where(g => g.Status == GoalStatus.Active).ToList();
        Goals.Clear();
        foreach (var g in activeGoals)
            Goals.Add(new GoalCardRow(g, BecomeStore.EstimateProgress(d, g)));
        HasGoals = Goals.Count > 0;

        var activeIds = activeGoals.Select(g => g.Id).ToHashSet();
        TodayActions.Clear();
        foreach (var a in d.Actions.Where(a => a.ScheduledDate.Date == DateTime.Today && activeIds.Contains(a.GoalId)))
        {
            var goalTitle = activeGoals.First(g => g.Id == a.GoalId).Title;
            TodayActions.Add(new TodayActionRow(a, goalTitle, ToggleAsync, EditAsync));
        }
        RefreshToday();
    }

    private void RefreshToday()
    {
        var total = TodayActions.Count;
        var done = TodayActions.Count(x => x.IsDone);

        TodayProgress = total == 0 ? 0 : (double)done / total;
        TodayText = total == 0 ? ""
            : done == total ? "Tout est fait pour aujourd'hui."
            : $"{done} sur {total} réalisée(s)";
        HasNoActions = HasGoals && total == 0;
    }

    private async Task ToggleAsync(TodayActionRow row)
    {
        var d = await _store.GetAsync();
        var a = d.Actions.FirstOrDefault(x => x.Id == row.Id);
        if (a is null) return;

        a.IsDone = !a.IsDone;
        a.DoneAt = a.IsDone ? DateTime.Now : null;
        await _store.SaveAsync();

        row.IsDone = a.IsDone;
        RefreshToday();
    }

    private async Task EditAsync(TodayActionRow row) =>
        await Shell.Current.GoToAsync($"{nameof(Views.ActionEditPage)}?id={row.Id}");
}