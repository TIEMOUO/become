using System.Collections.ObjectModel;
using System.Windows.Input;
using Become.Domain;
using Become.Services;

namespace Become.ViewModels;

public class GoalSuggestionRow : BecomeViewModelBase
{
    private string _title = "", _description = "", _expectedResult = "", _visionLink = "";
    private List<ActionSuggestion> _actions = new();
    private bool _isEditing;

    public string Domain { get; }
    public int Variant { get; set; }

    public string Title { get => _title; set => Set(ref _title, value); }
    public string Description { get => _description; set => Set(ref _description, value); }
    public string ExpectedResult { get => _expectedResult; set => Set(ref _expectedResult, value); }
    public string VisionLink { get => _visionLink; set => Set(ref _visionLink, value); }
    public List<ActionSuggestion> Actions => _actions;

    public string ActionsSummary =>
        string.Join("\n", _actions.Select(a => $"• {a.Title} ({a.Minutes} min)"));

    public bool IsEditing
    {
        get => _isEditing;
        set
        {
            if (Set(ref _isEditing, value))
            {
                OnPropertyChanged(nameof(IsNotEditing));
                OnPropertyChanged(nameof(EditButtonText));
            }
        }
    }
    public bool IsNotEditing => !IsEditing;
    public string EditButtonText => IsEditing ? "Terminer" : "Modifier";

    public ICommand AcceptCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand ReplaceCommand { get; }
    public ICommand RejectCommand { get; }

    public GoalSuggestionRow(GoalSuggestion suggestion,
        Func<GoalSuggestionRow, Task> accept,
        Func<GoalSuggestionRow, Task> replace,
        Func<GoalSuggestionRow, Task> reject)
    {
        Domain = suggestion.Domain;
        Apply(suggestion);

        AcceptCommand = new Command(async () => await accept(this));
        ReplaceCommand = new Command(async () => await replace(this));
        RejectCommand = new Command(async () => await reject(this));
        EditCommand = new Command(() => IsEditing = !IsEditing);
    }

    public void Apply(GoalSuggestion s)
    {
        Title = s.Title;
        Description = s.Description;
        ExpectedResult = s.ExpectedResult;
        VisionLink = s.VisionLink;
        _actions = s.Actions;
        OnPropertyChanged(nameof(Actions));
        OnPropertyChanged(nameof(ActionsSummary));
    }
}

public class GoalSuggestionsViewModel : BecomeViewModelBase
{
    private readonly IBecomeStore _store;
    private int _planned = 1;
    private int _activeCount;
    private string _message = "", _error = "";

    public ObservableCollection<GoalSuggestionRow> Suggestions { get; } = new();

    public string Message
    {
        get => _message;
        set { if (Set(ref _message, value)) OnPropertyChanged(nameof(HasMessage)); }
    }
    public bool HasMessage => !string.IsNullOrEmpty(Message);

    public string Error
    {
        get => _error;
        set { if (Set(ref _error, value)) OnPropertyChanged(nameof(HasError)); }
    }
    public bool HasError => !string.IsNullOrEmpty(Error);

    public string ActiveCountText => $"Objectifs actifs : {_activeCount} sur {BecomeStore.MaxActiveGoals}";
    public bool CanFinish => _activeCount > 0;

    public ICommand FinishCommand { get; }
    public ICommand EditPrioritiesCommand { get; }

    public GoalSuggestionsViewModel(IBecomeStore store)
    {
        _store = store;
        FinishCommand = new Command(async () => await Shell.Current.GoToAsync("//MainTabs"));
        EditPrioritiesCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(Views.PrioritiesPage)));
    }

    public async Task LoadAsync()
    {
        var d = await _store.GetAsync();
        Suggestions.Clear();
        Error = "";
        Message = "";
        RefreshCounts(d);

        if (d.Vision is null || d.Priorities is null)
        {
            Message = "Il manque ta vision ou tes priorités pour générer des propositions.";
            return;
        }

        var slots = BecomeStore.MaxActiveGoals - _activeCount;
        var activeDomains = d.Goals
            .Where(g => g.Status == GoalStatus.Active)
            .Select(g => g.Domain)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var domains = d.Priorities.Domains
            .Where(x => !activeDomains.Contains(x))
            .Take(Math.Max(0, slots))
            .ToList();

        if (domains.Count == 0)
        {
            Message = slots <= 0
                ? "Tu as déjà trois objectifs actifs. Termine ou archive-en un pour en ajouter."
                : "Tous tes domaines prioritaires ont déjà un objectif actif. Modifie tes priorités pour en explorer d'autres.";
            return;
        }

        _planned = domains.Count;
        foreach (var domain in domains)
        {
            var suggestion = GoalEngine.Suggest(domain, 0, d.Vision, d.Priorities, _planned);
            Suggestions.Add(new GoalSuggestionRow(suggestion, AcceptAsync, ReplaceAsync, RejectAsync));
        }
    }

    private void RefreshCounts(UserData d)
    {
        _activeCount = d.Goals.Count(g => g.Status == GoalStatus.Active);
        OnPropertyChanged(nameof(ActiveCountText));
        OnPropertyChanged(nameof(CanFinish));
    }

    private async Task AcceptAsync(GoalSuggestionRow row)
    {
        Error = "";
        if (string.IsNullOrWhiteSpace(row.Title))
        {
            Error = "Donne un titre à cet objectif.";
            return;
        }

        var goal = new LifeGoal
        {
            Title = row.Title.Trim(),
            Description = row.Description,
            Domain = row.Domain,
            VisionLink = row.VisionLink,
            ExpectedResult = (row.ExpectedResult ?? "").Trim()
        };

        var (ok, error) = await _store.AddGoalAsync(goal);
        if (!ok)
        {
            Error = error ?? "Impossible d'ajouter cet objectif.";
            return;
        }

        var skipped = 0;
        foreach (var a in row.Actions)
        {
            var result = await _store.AddActionAsync(new DailyAction
            {
                GoalId = goal.Id,
                Title = a.Title,
                Description = a.Description,
                ScheduledDate = DateTime.Today,
                EstimatedMinutes = a.Minutes,
                Frequency = "Quotidienne"
            });
            if (!result.Ok) skipped++;
        }

        if (skipped > 0)
            Error = "Limite de 5 actions par jour atteinte : certaines actions n'ont pas été planifiées.";

        Suggestions.Remove(row);
        RefreshCounts(await _store.GetAsync());

        if (Suggestions.Count == 0 && !HasMessage)
            Message = "Tes objectifs sont activés. Tu peux accéder à ton tableau de bord.";
    }

    private async Task ReplaceAsync(GoalSuggestionRow row)
    {
        var d = await _store.GetAsync();
        if (d.Vision is null || d.Priorities is null) return;

        row.Variant++;
        row.IsEditing = false;
        row.Apply(GoalEngine.Suggest(row.Domain, row.Variant, d.Vision, d.Priorities, _planned));
    }

    private Task RejectAsync(GoalSuggestionRow row)
    {
        Suggestions.Remove(row);
        if (Suggestions.Count == 0 && !HasMessage)
            Message = "Tu as refusé toutes les propositions. Modifie tes priorités pour en obtenir de nouvelles.";
        return Task.CompletedTask;
    }
}