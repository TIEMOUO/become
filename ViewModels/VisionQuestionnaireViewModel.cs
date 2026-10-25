using System.Windows.Input;
using Become.Domain;
using Become.Services;

namespace Become.ViewModels;

public class VisionQuestionnaireViewModel : BecomeViewModelBase
{
    private readonly IBecomeStore _store;
    private readonly string[] _answers = { "", "", "", "" };
    private int _step;
    private string _error = "";

    private static readonly string[] Questions =
    {
        "Quelle personne veux-tu devenir ?",
        "Quels domaines de ta vie sont importants pour toi ?",
        "Quelles sont tes valeurs et tes aspirations ?",
        "Quelle vie souhaites-tu construire ?"
    };

    private static readonly string[] Hints =
    {
        "Décris-la avec tes mots : son caractère, ce qu'elle fait, sa manière de vivre.",
        "Études, travail, famille, foi, santé, argent… écris ce qui compte vraiment.",
        "Ce qui te guide et ce que tu espères profondément.",
        "Imagine ta vie dans quelques années : à quoi ressemble-t-elle ?"
    };

    public string Question => Questions[_step];
    public string Hint => Hints[_step];
    public string StepLabel => $"ÉTAPE {_step + 1} SUR 4";
    public double Progress => (_step + 1) / 4.0;
    public bool CanGoBack => _step > 0;
    public string NextText => _step == 3 ? "Voir ma vision" : "Continuer";

    public string CurrentAnswer
    {
        get => _answers[_step];
        set { _answers[_step] = value ?? ""; OnPropertyChanged(); }
    }

    public string Error
    {
        get => _error;
        set { if (Set(ref _error, value)) OnPropertyChanged(nameof(HasError)); }
    }
    public bool HasError => !string.IsNullOrEmpty(Error);

    public ICommand NextCommand { get; }
    public ICommand PreviousCommand { get; }

    public VisionQuestionnaireViewModel(IBecomeStore store)
    {
        _store = store;
        NextCommand = new Command(async () => await NextAsync());
        PreviousCommand = new Command(() => { if (_step > 0) { _step--; RefreshStep(); } });
    }

    public async Task LoadAsync()
    {
        var d = await _store.GetAsync();
        if (d.Vision is not null && _answers.All(string.IsNullOrWhiteSpace))
        {
            _answers[0] = d.Vision.WhoIWantToBecome;
            _answers[1] = d.Vision.ImportantAreas;
            _answers[2] = d.Vision.ValuesAndAspirations;
            _answers[3] = d.Vision.LifeIWantToBuild;
            RefreshStep();
        }
    }

    private void RefreshStep()
    {
        Error = "";
        OnPropertyChanged(nameof(Question));
        OnPropertyChanged(nameof(Hint));
        OnPropertyChanged(nameof(StepLabel));
        OnPropertyChanged(nameof(Progress));
        OnPropertyChanged(nameof(CanGoBack));
        OnPropertyChanged(nameof(NextText));
        OnPropertyChanged(nameof(CurrentAnswer));
    }

    private async Task NextAsync()
    {
        if (_answers[_step].Trim().Length < 10)
        {
            Error = "Écris au moins une phrase pour continuer (10 caractères minimum).";
            return;
        }

        if (_step < 3)
        {
            _step++;
            RefreshStep();
            return;
        }

        var d = await _store.GetAsync();
        var v = d.Vision ?? new VisionProfile();

        // L'historique n'est jamais supprimé : on conserve l'ancienne version validée
        if (v.IsValidated && !string.IsNullOrWhiteSpace(v.VisionText))
            d.VisionHistory.Add(new VisionVersion { Text = v.VisionText, Date = DateTime.UtcNow });

        v.WhoIWantToBecome = _answers[0].Trim();
        v.ImportantAreas = _answers[1].Trim();
        v.ValuesAndAspirations = _answers[2].Trim();
        v.LifeIWantToBuild = _answers[3].Trim();
        v.VisionText = VisionWriter.Compose(v);
        v.IsValidated = false;
        v.UpdatedAt = DateTime.UtcNow;

        d.Vision = v;
        await _store.SaveAsync();
        await Shell.Current.GoToAsync(nameof(Views.VisionResultPage));
    }
}