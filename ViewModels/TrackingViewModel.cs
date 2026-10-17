using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Become.Helpers;
using Become.Models;
using Become.Services;

namespace Become.ViewModels;

/// <summary>Une action à valider aujourd'hui (une ligne de la liste).</summary>
public class TaskItem : INotifyPropertyChanged
{
    private bool _isDone;

    public TaskItem(Goal goal)
    {
        Goal = goal;
        _isDone = goal.IsDoneOn(DateTime.Today);
    }

    public Goal Goal { get; }
    public string Title => Goal.Title;
    public string Icon => Goal.Icon;
    public string Detail => $"{Goal.SessionMinutes} min • {Goal.ProgressText}";

    public bool IsDone
    {
        get => _isDone;
        set
        {
            if (_isDone == value) return;
            _isDone = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDone)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Detail)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StateText)));
        }
    }

    /// <summary>Le symbole porte l'état (pas seulement la couleur).</summary>
    public string StateText => IsDone ? "✓" : "";

    public string AccessibleLabel => $"{Title}, {(IsDone ? "validée" : "à faire")}";

    public event PropertyChangedEventHandler? PropertyChanged;
}

/// <summary>Un jour de la rangée des 7 derniers jours.</summary>
public record DayStatus(string Letter, string Number, bool IsDone, bool IsToday);

public class TrackingViewModel : FeedbackViewModel
{
    private readonly IGoalStore _store;
    private int _streak;

    public TrackingViewModel(IGoalStore store)
    {
        _store = store;
        CreateGoalCommand = new Command(async () => await AppNavigator.OpenCreateGoalAsync());
    }

    public ObservableCollection<TaskItem> Tasks { get; } = new();
    public ObservableCollection<DayStatus> Week { get; } = new();

    public string DateTitle => Fmt.Cap(DateTime.Today.ToString("dddd d MMMM", Fmt.Fr));

    // ----- Série -----
    public string StreakText => _streak switch
    {
        0 => "Aucune série en cours",
        1 => "1 jour de suite",
        _ => $"{_streak} jours de suite"
    };

    // ----- Progression du jour -----
    private int DoneCount => Tasks.Count(t => t.IsDone);
    public string CounterText => $"{DoneCount} / {Tasks.Count} validé{(DoneCount > 1 ? "s" : "")}";

    // ----- États -----
    public bool HasNoGoal => _store.Goals.Count(g => !g.IsFinished) == 0;
    public bool NothingDueToday => !HasNoGoal && Tasks.Count == 0;
    public bool HasTasks => Tasks.Count > 0;
    public bool AllDone => Tasks.Count > 0 && DoneCount == Tasks.Count;

    public ICommand CreateGoalCommand { get; }

    public void Refresh()
    {
        Tasks.Clear();
        foreach (var g in _store.DueToday()) Tasks.Add(new TaskItem(g));

        _streak = _store.GlobalStreak();
        BuildWeek();
        OnPropertyChanged(string.Empty);
    }

    /// <summary>Appui sur une ligne : valide la séance du jour, ou l'annule si elle l'était déjà.</summary>
    public void Toggle(TaskItem item)
    {
        var result = _store.ToggleToday(item.Goal);
        item.IsDone = result.Validated;

        _streak = _store.GlobalStreak();
        BuildWeek();
        OnPropertyChanged(string.Empty);
        ShowFeedback(Describe(result, item.Goal));
    }

    private void BuildWeek()
    {
        Week.Clear();
        var doneDays = _store.Goals.SelectMany(g => g.DoneDays).ToHashSet();
        for (var i = 6; i >= 0; i--)
        {
            var d = DateTime.Today.AddDays(-i);
            Week.Add(new DayStatus(Fmt.Letter(d.DayOfWeek), d.Day.ToString(), doneDays.Contains(d), i == 0));
        }
    }
}
