using System.Collections.ObjectModel;
using System.Windows.Input;
using Become.Domain;
using Become.Services;

namespace Become.ViewModels;

public class GoalsViewModel : BecomeViewModelBase
{
    private readonly IBecomeStore _store;
    private string _countText = "";
    private int _achieved;
    private bool _canAdd, _hasGoals;

    public ObservableCollection<GoalCardRow> Goals { get; } = new();

    public string CountText { get => _countText; set => Set(ref _countText, value); }
    public string AchievedText => $"Objectifs atteints : {_achieved}";
    public bool HasAchieved => _achieved > 0;
    public bool CanAdd { get => _canAdd; set => Set(ref _canAdd, value); }

    public bool HasGoals
    {
        get => _hasGoals;
        set { if (Set(ref _hasGoals, value)) OnPropertyChanged(nameof(HasNoGoals)); }
    }
    public bool HasNoGoals => !HasGoals;

    public ICommand AddGoalCommand { get; }

    public GoalsViewModel(IBecomeStore store)
    {
        _store = store;
        AddGoalCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(Views.GoalSuggestionsPage)));
    }

    public async Task LoadAsync()
    {
        var d = await _store.GetAsync();
        var active = d.Goals.Where(g => g.Status == GoalStatus.Active).ToList();

        Goals.Clear();
        foreach (var g in active)
            Goals.Add(new GoalCardRow(g, BecomeStore.EstimateProgress(d, g), EditAsync, AchieveAsync, ArchiveAsync));

        HasGoals = Goals.Count > 0;
        CountText = $"{active.Count} sur {BecomeStore.MaxActiveGoals} objectifs actifs";
        CanAdd = active.Count < BecomeStore.MaxActiveGoals;
        _achieved = d.Goals.Count(g => g.Status == GoalStatus.Achieved);
        OnPropertyChanged(nameof(AchievedText));
        OnPropertyChanged(nameof(HasAchieved));
    }

    private async Task EditAsync(GoalCardRow row) =>
        await Shell.Current.GoToAsync($"{nameof(Views.GoalEditPage)}?id={row.Id}");

    private async Task AchieveAsync(GoalCardRow row)
    {
        var confirm = await Shell.Current.DisplayAlert(
            "Objectif atteint ?",
            "Marque-le comme atteint seulement si le résultat attendu est vraiment obtenu. C'est toi qui décides.",
            "Oui, il est atteint", "Pas encore");
        if (!confirm) return;
        await SetStatusAsync(row, GoalStatus.Achieved);
    }

    private async Task ArchiveAsync(GoalCardRow row)
    {
        var confirm = await Shell.Current.DisplayAlert(
            "Archiver cet objectif ?",
            "Il ne sera plus actif, mais son historique est conservé.",
            "Archiver", "Annuler");
        if (!confirm) return;
        await SetStatusAsync(row, GoalStatus.Archived);
    }

    private async Task SetStatusAsync(GoalCardRow row, GoalStatus status)
    {
        var d = await _store.GetAsync();
        var g = d.Goals.FirstOrDefault(x => x.Id == row.Id);
        if (g is null) return;
        g.Status = status;
        await _store.SaveAsync();
        await LoadAsync();
    }
}