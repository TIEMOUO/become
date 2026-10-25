using System.Windows.Input;
using Become.Domain;
using Become.Services;

namespace Become.ViewModels;

public class PrioritiesViewModel : BecomeViewModelBase
{
    private readonly IBecomeStore _store;
    private readonly SelectableChip _other = new("Autre");
    private double _minutes = 30;
    private string _customDomain = "", _constraints = "", _error = "";

    public List<SelectableChip> Domains { get; } = new();

    public bool ShowOtherField => _other.IsSelected;

    public string CustomDomain { get => _customDomain; set => Set(ref _customDomain, value); }
    public string Constraints { get => _constraints; set => Set(ref _constraints, value); }

    public double Minutes
    {
        get => _minutes;
        set
        {
            _minutes = Math.Round(value / 5.0) * 5;
            OnPropertyChanged();
            OnPropertyChanged(nameof(MinutesText));
        }
    }
    public string MinutesText => $"{(int)Minutes} minutes par jour";

    public string Error
    {
        get => _error;
        set { if (Set(ref _error, value)) OnPropertyChanged(nameof(HasError)); }
    }
    public bool HasError => !string.IsNullOrEmpty(Error);

    public ICommand ContinueCommand { get; }

    public PrioritiesViewModel(IBecomeStore store)
    {
        _store = store;

        foreach (var d in LifeDomains.Standard) Domains.Add(new SelectableChip(d));
        Domains.Add(_other);

        _other.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SelectableChip.IsSelected))
                OnPropertyChanged(nameof(ShowOtherField));
        };

        ContinueCommand = new Command(async () => await ContinueAsync());
    }

    public async Task LoadAsync()
    {
        var d = await _store.GetAsync();
        var p = d.Priorities;
        if (p is null) return;

        foreach (var chip in Domains) chip.IsSelected = p.Domains.Contains(chip.Label);

        var custom = p.Domains.FirstOrDefault(x => !LifeDomains.Standard.Contains(x));
        if (custom is not null)
        {
            _other.IsSelected = true;
            CustomDomain = custom;
        }

        Minutes = p.MinutesPerDay > 0 ? p.MinutesPerDay : 30;
        Constraints = p.Constraints;
    }

    private async Task ContinueAsync()
    {
        Error = "";
        var selected = Domains.Where(c => c.IsSelected && c != _other).Select(c => c.Label).ToList();

        if (_other.IsSelected)
        {
            var customName = (CustomDomain ?? "").Trim();
            if (customName.Length < 2)
            {
                Error = "Précise ton autre domaine pour que je puisse proposer des objectifs adaptés.";
                return;
            }
            selected.Add(customName);
        }

        if (selected.Count == 0)
        {
            Error = "Choisis au moins un domaine à faire avancer.";
            return;
        }
        if (selected.Count > 3)
        {
            Error = "Choisis 3 domaines au maximum pour rester concentré(e).";
            return;
        }

        var d = await _store.GetAsync();
        d.Priorities = new PrioritiesProfile
        {
            Domains = selected,
            MinutesPerDay = (int)Minutes,
            Constraints = (Constraints ?? "").Trim(),
            UpdatedAt = DateTime.UtcNow
        };
        await _store.SaveAsync();

        if (d.Goals.Any(g => g.Status == GoalStatus.Active))
            await Shell.Current.GoToAsync("//MainTabs");
        else
            await Shell.Current.GoToAsync(nameof(Views.GoalSuggestionsPage));
    }
}