using System.Collections.ObjectModel;
using System.Windows.Input;
using Become.Helpers;
using Become.Models;
using Become.Services;

namespace Become.ViewModels;

public record MilestoneItem(string Title, string Detail, bool IsReached);

public record SessionItem(string Label, string Detail);

public class GoalDetailViewModel : FeedbackViewModel, IQueryAttributable
{
    private readonly IGoalStore _store;

    private Goal? _goal;
    private bool _showAllSessions;

    public GoalDetailViewModel(IGoalStore store)
    {
        _store = store;
        BackCommand = new Command(async () => await AppNavigator.BackAsync());
        ValidateCommand = new Command(Validate);
        MoreCommand = new Command(async () => await ShowMoreAsync());
        ToggleSessionsCommand = new Command(() => { _showAllSessions = !_showAllSessions; Refresh(); });
    }

    public ObservableCollection<MilestoneItem> Milestones { get; } = new();
    public ObservableCollection<SessionItem> Sessions { get; } = new();

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var id) && id is string goalId)
        {
            _goal = _store.Get(goalId);
            Refresh();
        }
    }

    // ----- En-tête -----
    public bool HasGoal => _goal is not null;
    public string Title => _goal?.Title ?? "";
    public string Subtitle => _goal?.DetailSubtitle ?? "";

    // ----- Progression -----
    public double Progress => _goal?.Progress ?? 0;
    public string StatusText => _goal?.StatusText ?? "";
    public bool IsAchieved => _goal?.IsAchieved ?? false;
    public string CountText => _goal is null ? "" : $"{_goal.Done} / {_goal.TotalSessions}";

    public string RemainingText
    {
        get
        {
            if (_goal is null) return "";
            if (_goal.IsAchieved) return "Objectif atteint. Bravo, vous l'avez fait !";
            if (_goal.IsFinished) return $"Échéance dépassée : il restait {_goal.Remaining} séances.";
            return $"Encore {_goal.Remaining} séance{(_goal.Remaining > 1 ? "s" : "")} pour atteindre votre objectif.";
        }
    }

    public bool HasWhy => !string.IsNullOrWhiteSpace(_goal?.Why);
    public string WhyQuote => $"« {_goal?.Why} »";

    // ----- Régularité -----
    private int CurrentStreak => _goal is null ? 0 : Stats.CurrentStreak(_goal.DoneDays, DateTime.Today);
    private int BestStreak => _goal is null ? 0 : Stats.BestStreak(_goal.DoneDays);

    public string StreakText => CurrentStreak switch
    {
        0 => "Aucune série en cours",
        1 => "1 jour consécutif",
        var n => $"{n} jours consécutifs"
    };
    public string BestStreakText => $"Votre meilleure série : {BestStreak} jour{(BestStreak > 1 ? "s" : "")}";

    // ----- Jalons -----
    public string MilestonesCounter =>
        _goal is null ? "" : $"{_goal.Milestones.Count(m => _goal.Done >= m.Sessions)} / {_goal.Milestones.Count} atteints";
    public bool HasMilestones => Milestones.Count > 0;

    // ----- Séances récentes -----
    public bool HasSessions => _goal?.Sessions.Count > 0;
    public bool HasNoSessions => !HasSessions;
    public bool CanToggleSessions => (_goal?.Sessions.Count ?? 0) > 2;
    public string ToggleSessionsText => _showAllSessions ? "Réduire" : "Tout voir";

    // ----- Rappel -----
    public bool ReminderEnabled
    {
        get => _goal?.ReminderEnabled ?? false;
        set
        {
            if (_goal is null || _goal.ReminderEnabled == value) return;
            _goal.ReminderEnabled = value;
            _store.Save();
            OnPropertyChanged();
            OnPropertyChanged(nameof(ReminderText));
        }
    }

    public string ReminderText => _goal is null ? "" :
        _goal.ReminderEnabled
            ? $"Rappel à {_goal.ReminderTime.ToString(@"hh\:mm")} les jours de séance"
            : "Rappel désactivé";

    // ----- Validation du jour -----
    private bool DoneToday => _goal?.IsDoneOn(DateTime.Today) ?? false;
    public bool CanValidate => _goal is not null && !_goal.IsFinished && !DoneToday;
    public bool IsValidatedToday => _goal is not null && !_goal.IsFinished && DoneToday;
    public bool IsFinished => _goal?.IsFinished ?? false;

    public ICommand BackCommand { get; }
    public ICommand ValidateCommand { get; }
    public ICommand MoreCommand { get; }
    public ICommand ToggleSessionsCommand { get; }

    private void Validate()
    {
        if (_goal is null || _goal.IsFinished) return;
        var result = _store.ToggleToday(_goal);
        Refresh();
        ShowFeedback(Describe(result, _goal));
    }

    private async Task ShowMoreAsync()
    {
        if (_goal is null) return;
        var choice = await Shell.Current.DisplayActionSheet(_goal.Title, "Fermer", null, "Supprimer l'objectif");
        if (choice != "Supprimer l'objectif") return;

        var confirm = await Shell.Current.DisplayAlert("Supprimer cet objectif ?",
            "Sa progression et son historique seront définitivement effacés.", "Supprimer", "Garder");
        if (!confirm) return;

        _store.Remove(_goal.Id);
        await AppNavigator.BackAsync();
    }

    private void Refresh()
    {
        Milestones.Clear();
        Sessions.Clear();

        if (_goal is not null)
        {
            var ordered = _goal.Sessions.OrderBy(s => s.Date).ToList();

            foreach (var m in _goal.Milestones)
            {
                var reached = _goal.Done >= m.Sessions;
                var detail = reached
                    ? $"{m.Sessions} séances • atteint le {Fmt.Short(ordered[m.Sessions - 1].Date)}"
                    : $"{m.Sessions} séances • encore {m.Sessions - _goal.Done} séance{(m.Sessions - _goal.Done > 1 ? "s" : "")}";
                Milestones.Add(new MilestoneItem(m.Title, detail, reached));
            }

            var recent = ordered.AsEnumerable().Reverse();
            if (!_showAllSessions) recent = recent.Take(2);
            foreach (var s in recent)
                Sessions.Add(new SessionItem(Fmt.Relative(s.Date), $"{s.Minutes} min • séance validée"));
        }

        OnPropertyChanged(string.Empty);
    }
}
