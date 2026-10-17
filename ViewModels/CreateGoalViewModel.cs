using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Become.Helpers;
using Become.Models;
using Become.Services;

namespace Become.ViewModels;

/// <summary>Une pastille de jour (L M M J V S D).</summary>
public class DayChip : INotifyPropertyChanged
{
    private bool _isSelected;

    public DayChip(DayOfWeek day, string letter, bool selected)
    {
        Day = day;
        Letter = letter;
        _isSelected = selected;
    }

    public DayOfWeek Day { get; }
    public string Letter { get; }
    public string Description => Fmt.Cap(Fmt.DayName(Day));

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}

public class CreateGoalViewModel : BaseViewModel
{
    private readonly IGoalStore _store;

    private string _title = "";
    private string _why = "";
    private string _category = Categories.Names[0];
    private Frequency _frequency = Frequency.Weekly;
    private string _duration = "15 minutes";
    private DateTime _targetDate = DateTime.Today.AddDays(30);
    private bool _reminderEnabled = true;
    private TimeSpan _reminderTime = new(18, 0, 0);
    private string _titleError = "";
    private string _daysError = "";
    private string _dateError = "";
    private bool _isBusy;

    public CreateGoalViewModel(IGoalStore store)
    {
        _store = store;

        // Valeurs par défaut intelligentes : lundi, mercredi, vendredi
        foreach (var (day, letter) in Fmt.WeekDays)
        {
            var chip = new DayChip(day, letter,
                day is DayOfWeek.Monday or DayOfWeek.Wednesday or DayOfWeek.Friday);
            chip.PropertyChanged += (_, _) => { DaysError = ""; NotifyPlan(); };
            DayChips.Add(chip);
        }

        SetFrequencyCommand = new Command<string>(SetFrequency);
        SaveCommand = new Command(async () => await SaveAsync(), () => !IsBusy);
        CancelCommand = new Command(async () => await CancelAsync());
    }

    public ObservableCollection<DayChip> DayChips { get; } = new();
    public IList<string> CategoryOptions => Categories.Names;
    public IList<string> Durations { get; } =
        new List<string> { "5 minutes", "10 minutes", "15 minutes", "20 minutes", "30 minutes", "45 minutes", "60 minutes" };

    // ----- Champs -----
    public string Title
    {
        get => _title;
        set { if (SetProperty(ref _title, value)) TitleError = ""; }
    }

    public string Why { get => _why; set => SetProperty(ref _why, value); }

    public string Category { get => _category; set => SetProperty(ref _category, value); }

    public string Duration
    {
        get => _duration;
        set { if (value is not null) SetProperty(ref _duration, value); }
    }

    public DateTime TargetDate
    {
        get => _targetDate;
        set
        {
            if (!SetProperty(ref _targetDate, value)) return;
            DateError = "";
            NotifyPlan();
        }
    }

    public DateTime MinimumDate => DateTime.Today.AddDays(1);

    public bool ReminderEnabled
    {
        get => _reminderEnabled;
        set { if (SetProperty(ref _reminderEnabled, value)) OnPropertyChanged(nameof(ReminderTitle)); }
    }

    public TimeSpan ReminderTime
    {
        get => _reminderTime;
        set { if (SetProperty(ref _reminderTime, value)) OnPropertyChanged(nameof(ReminderTitle)); }
    }

    public string ReminderTitle => ReminderEnabled ? $"Me rappeler à {ReminderTime.ToString(@"hh\:mm")}" : "Me rappeler";

    // ----- Fréquence -----
    public bool IsDaily => _frequency == Frequency.Daily;
    public bool IsWeekly => _frequency == Frequency.Weekly;
    public bool IsFree => _frequency == Frequency.Free;

    private void SetFrequency(string? key)
    {
        _frequency = key switch
        {
            "daily" => Frequency.Daily,
            "free" => Frequency.Free,
            _ => Frequency.Weekly
        };
        DaysError = "";
        OnPropertyChanged(nameof(IsDaily));
        OnPropertyChanged(nameof(IsWeekly));
        OnPropertyChanged(nameof(IsFree));
        NotifyPlan();
    }

    private List<DayOfWeek> SelectedDays => DayChips.Where(c => c.IsSelected).Select(c => c.Day).ToList();

    private int PlannedSessions => Goal.CountScheduled(_frequency, SelectedDays, DateTime.Today, TargetDate);

    public string PlanText
    {
        get
        {
            var rhythm = _frequency switch
            {
                Frequency.Daily => "Une séance par jour",
                Frequency.Free => "Quand vous voulez, sans jour imposé",
                _ => SelectedDays.Count == 0
                    ? "Choisissez au moins un jour"
                    : $"{SelectedDays.Count} séance{(SelectedDays.Count > 1 ? "s" : "")} par semaine • " +
                      string.Join(", ", DayChips.Where(c => c.IsSelected).Select(c => Fmt.DayName(c.Day)))
            };
            return $"{rhythm}\n≈ {PlannedSessions} séances d'ici le {Fmt.Short(TargetDate)}";
        }
    }

    private void NotifyPlan() => OnPropertyChanged(nameof(PlanText));

    // ----- Erreurs -----
    public string TitleError { get => _titleError; private set => SetProperty(ref _titleError, value); }

    public string DaysError
    {
        get => _daysError;
        private set { if (SetProperty(ref _daysError, value)) OnPropertyChanged(nameof(HasDaysError)); }
    }
    public bool HasDaysError => !string.IsNullOrEmpty(DaysError);

    public string DateError
    {
        get => _dateError;
        private set { if (SetProperty(ref _dateError, value)) OnPropertyChanged(nameof(HasDateError)); }
    }
    public bool HasDateError => !string.IsNullOrEmpty(DateError);

    // ----- Commandes -----
    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (!SetProperty(ref _isBusy, value)) return;
            OnPropertyChanged(nameof(ButtonText));
            ((Command)SaveCommand).ChangeCanExecute();
        }
    }

    public string ButtonText => IsBusy ? "Création en cours…" : "+  Créer mon objectif";

    public ICommand SetFrequencyCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public void ToggleDay(DayChip chip) => chip.IsSelected = !chip.IsSelected;

    private async Task SaveAsync()
    {
        TitleError = string.IsNullOrWhiteSpace(Title) ? "Donnez un nom à votre objectif." : "";
        DaysError = _frequency == Frequency.Weekly && SelectedDays.Count == 0
            ? "Choisissez au moins un jour de séance." : "";
        DateError = TargetDate.Date <= DateTime.Today
            ? "Choisissez une date dans le futur."
            : PlannedSessions == 0 ? "Aucune séance prévue avant cette date : choisissez une date plus lointaine." : "";

        if (TitleError.Length + DaysError.Length + DateError.Length > 0) return;

        IsBusy = true;
        try
        {
            var total = PlannedSessions;
            var minutes = int.TryParse(Duration.Split(' ')[0], out var m) ? m : 15;

            var goal = new Goal
            {
                Title = Title.Trim(),
                Why = Why.Trim(),
                Category = Category,
                Frequency = _frequency,
                WeekDays = _frequency == Frequency.Weekly ? SelectedDays : new(),
                SessionMinutes = minutes,
                StartDate = DateTime.Today,
                TargetDate = TargetDate.Date,
                ReminderEnabled = ReminderEnabled,
                ReminderTime = ReminderTime,
                TotalSessions = total,
                Milestones = Goal.BuildMilestones(total)
            };

            _store.Add(goal);
            await Shell.Current.GoToAsync($"../{AppNavigator.DetailRoute}?id={goal.Id}");
        }
        catch
        {
            await Shell.Current.DisplayAlert("Création impossible",
                "Votre objectif n'a pas pu être enregistré. Réessayez.", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Retour : demande confirmation si une saisie est en cours.</summary>
    public async Task CancelAsync()
    {
        var dirty = !string.IsNullOrWhiteSpace(Title) || !string.IsNullOrWhiteSpace(Why);
        if (dirty)
        {
            var leave = await Shell.Current.DisplayAlert("Abandonner la création ?",
                "Votre saisie ne sera pas enregistrée.", "Abandonner", "Continuer");
            if (!leave) return;
        }
        await AppNavigator.BackAsync();
    }
}
