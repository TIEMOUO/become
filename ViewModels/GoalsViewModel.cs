using System.Collections.ObjectModel;
using System.Windows.Input;
using Become.Helpers;
using Become.Models;
using Become.Services;

namespace Become.ViewModels;

public class GoalsViewModel : BaseViewModel
{
    private readonly IGoalStore _store;

    private string _search = "";
    private string _filter = "all";   // all | active | finished

    public GoalsViewModel(IGoalStore store)
    {
        _store = store;
        SetFilterCommand = new Command<string>(f => { Filter = f ?? "all"; });
        CreateGoalCommand = new Command(async () => await AppNavigator.OpenCreateGoalAsync());
        OpenGoalCommand = new Command<Goal>(async g => { if (g is not null) await AppNavigator.OpenGoalAsync(g.Id); });
    }

    public ObservableCollection<Goal> ActiveGoals { get; } = new();
    public ObservableCollection<Goal> FinishedGoals { get; } = new();

    public string Search
    {
        get => _search;
        set { if (SetProperty(ref _search, value)) Refresh(); }
    }

    public string Filter
    {
        get => _filter;
        private set { if (SetProperty(ref _filter, value)) Refresh(); }
    }

    // ----- Compteurs et état des filtres -----
    public string AllChip => $"Tous · {_store.Goals.Count}";
    public string ActiveChip => $"En cours · {_store.Goals.Count(g => !g.IsFinished)}";
    public string FinishedChip => $"Terminés · {_store.Goals.Count(g => g.IsFinished)}";
    public bool IsAllSelected => Filter == "all";
    public bool IsActiveSelected => Filter == "active";
    public bool IsFinishedSelected => Filter == "finished";

    // ----- Sections -----
    public bool ShowActive => ActiveGoals.Count > 0;
    public bool ShowFinished => FinishedGoals.Count > 0;
    public string ActiveCount => $"{ActiveGoals.Count} objectif{(ActiveGoals.Count > 1 ? "s" : "")}";
    public string FinishedTitle => Filter == "finished" ? "Terminés" : "Dernier objectif terminé";

    // ----- États vides -----
    public bool HasNoGoalAtAll => _store.Goals.Count == 0;
    public bool HasNoResult => _store.Goals.Count > 0 && ActiveGoals.Count == 0 && FinishedGoals.Count == 0;
    public bool HasGoals => _store.Goals.Count > 0;

    public ICommand SetFilterCommand { get; }
    public ICommand CreateGoalCommand { get; }
    public ICommand OpenGoalCommand { get; }

    public void Refresh()
    {
        var query = (Search ?? "").Trim();
        bool Matches(Goal g) => query.Length == 0 || g.Title.Contains(query, StringComparison.CurrentCultureIgnoreCase);

        ActiveGoals.Clear();
        FinishedGoals.Clear();

        if (Filter != "finished")
            foreach (var g in _store.Goals.Where(g => !g.IsFinished && Matches(g))) ActiveGoals.Add(g);

        if (Filter != "active")
        {
            var finished = _store.Goals.Where(g => g.IsFinished && Matches(g)).OrderByDescending(g => g.TargetDate);
            foreach (var g in Filter == "all" ? finished.Take(1) : finished) FinishedGoals.Add(g);
        }

        OnPropertyChanged(string.Empty);
    }
}
