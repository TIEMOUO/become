using System.Collections.ObjectModel;
using System.Windows.Input;
using Become.Helpers;
using Become.Models;
using Become.Services;

namespace Become.ViewModels;

public class HomeViewModel : BaseViewModel
{
    private readonly IGoalStore _store;
    private readonly IAuthService _auth;

    private bool _isLoading;
    private bool _hasError;
    private string _errorMessage = "";
    private int _streak;
    private IReadOnlyList<Goal> _due = Array.Empty<Goal>();

    public HomeViewModel(IGoalStore store, IAuthService auth)
    {
        _store = store;
        _auth = auth;

        RetryCommand = new Command(async () => await LoadAsync());
        OpenTrackingCommand = new Command(async () => await AppNavigator.OpenTabAsync("tracking"));
        OpenGoalsCommand = new Command(async () => await AppNavigator.OpenTabAsync("goals"));
        OpenGoalCommand = new Command<Goal>(async g => { if (g is not null) await AppNavigator.OpenGoalAsync(g.Id); });
        CreateGoalCommand = new Command(async () => await AppNavigator.OpenCreateGoalAsync());
        ProfileCommand = new Command(async () => await ShowProfileAsync());
    }

    /// <summary>Objectifs actifs (non terminés).</summary>
    public ObservableCollection<Goal> Goals { get; } = new();

    // ----- États d'écran -----
    public bool IsLoading { get => _isLoading; private set => SetProperty(ref _isLoading, value); }
    public bool HasError { get => _hasError; private set => SetProperty(ref _hasError, value); }
    public string ErrorMessage { get => _errorMessage; private set => SetProperty(ref _errorMessage, value); }
    public bool IsEmpty => !IsLoading && !HasError && Goals.Count == 0;
    public bool ShowContent => !IsLoading && Goals.Count > 0;

    // ----- En-tête -----
    private static string FirstName => Preferences.Get("first_name", "");
    public string DateText => DateTime.Now.ToString("dddd d MMMM", Fmt.Fr).ToUpper(Fmt.Fr);
    public string Greeting => string.IsNullOrWhiteSpace(FirstName) ? "Bonjour." : $"Bonjour {FirstName}.";
    public string Initial => string.IsNullOrWhiteSpace(FirstName) ? "?" : FirstName[..1].ToUpper(Fmt.Fr);

    // ----- Carte de synthèse -----
    private int TotalDone => Goals.Sum(g => g.Done);
    private int TotalPlanned => Goals.Sum(g => g.TotalSessions);
    public double OverallProgress => TotalPlanned == 0 ? 0 : (double)TotalDone / TotalPlanned;
    public string SummaryText => $"{TotalDone} séances réalisées sur {TotalPlanned} prévues.";
    public string StreakText => _streak switch
    {
        0 => "Commencez votre série aujourd'hui",
        1 => "1 jour de suite",
        _ => $"{_streak} jours de suite"
    };

    // ----- Aujourd'hui -----
    private Goal? NextTask => _due.FirstOrDefault(g => !g.IsDoneOn(DateTime.Today));
    private int DoneToday => _due.Count(g => g.IsDoneOn(DateTime.Today));
    public string TodayCounter => $"{DoneToday} / {_due.Count} validé{(DoneToday > 1 ? "s" : "")}";
    public bool HasPendingToday => NextTask is not null;
    public bool AllDoneToday => _due.Count > 0 && NextTask is null;
    public bool NothingToday => Goals.Count > 0 && _due.Count == 0;
    public string TodayTitle => NextTask?.Title ?? "";
    public string TodaySubtitle => NextTask is null ? "" : $"{NextTask.SessionMinutes} min • {NextTask.FrequencyLabel}";
    public string TodayIcon => NextTask?.Icon ?? "";
    public string GoalsLinkText => $"Voir les {Goals.Count}";

    // ----- Commandes -----
    public ICommand RetryCommand { get; }
    public ICommand OpenTrackingCommand { get; }
    public ICommand OpenGoalsCommand { get; }
    public ICommand OpenGoalCommand { get; }
    public ICommand CreateGoalCommand { get; }
    public ICommand ProfileCommand { get; }

    public async Task LoadAsync()
    {
        HasError = false;
        IsLoading = true;
        OnPropertyChanged(string.Empty);

        try
        {
            await Task.Yield();   // laisse l'écran se dessiner (squelettes) ; ici la lecture est locale

            Goals.Clear();
            foreach (var g in _store.Goals.Where(g => !g.IsFinished)) Goals.Add(g);
            _due = _store.DueToday();
            _streak = _store.GlobalStreak();
        }
        catch
        {
            HasError = true;
            ErrorMessage = "Impossible de charger vos objectifs. Réessayez.";
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(string.Empty);
        }
    }

    private async Task ShowProfileAsync()
    {
        var choice = await Shell.Current.DisplayActionSheet("Mon compte", "Fermer", null, "Se déconnecter");
        if (choice != "Se déconnecter") return;
        _auth.Logout();
        await AppNavigator.GoToLoginAsync();
    }
}
