using System.Windows.Input;
using Become.Domain;
using Become.Services;

namespace Become.ViewModels;

public class VisionResultViewModel : BecomeViewModelBase
{
    private readonly IBecomeStore _store;
    private string _visionText = "";
    private string _error = "";

    public string VisionText { get => _visionText; set => Set(ref _visionText, value); }

    public string Error
    {
        get => _error;
        set { if (Set(ref _error, value)) OnPropertyChanged(nameof(HasError)); }
    }
    public bool HasError => !string.IsNullOrEmpty(Error);

    public ICommand ValidateCommand { get; }
    public ICommand RegenerateCommand { get; }
    public ICommand ReviewAnswersCommand { get; }

    public VisionResultViewModel(IBecomeStore store)
    {
        _store = store;
        ValidateCommand = new Command(async () => await ValidateAsync());
        RegenerateCommand = new Command(async () => await RegenerateAsync());
        ReviewAnswersCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(Views.VisionQuestionnairePage)));
    }

    public async Task LoadAsync()
    {
        var d = await _store.GetAsync();
        VisionText = d.Vision?.VisionText ?? "";
        Error = "";
    }

    private async Task RegenerateAsync()
    {
        var d = await _store.GetAsync();
        if (d.Vision is null) return;
        VisionText = VisionWriter.Compose(d.Vision);
    }

    private async Task ValidateAsync()
    {
        var text = (VisionText ?? "").Trim();
        if (text.Length < 30)
        {
            Error = "Ta vision est un peu courte. Ajoute quelques phrases pour qu'elle te ressemble.";
            return;
        }

        var d = await _store.GetAsync();
        var v = d.Vision;
        if (v is null) return;

        if (v.IsValidated && v.VisionText != text)
            d.VisionHistory.Add(new VisionVersion { Text = v.VisionText, Date = DateTime.UtcNow });

        v.VisionText = text;
        v.IsValidated = true;
        v.UpdatedAt = DateTime.UtcNow;
        await _store.SaveAsync();

        if (d.Priorities is null)
            await Shell.Current.GoToAsync(nameof(Views.PrioritiesPage));
        else
            await Shell.Current.GoToAsync("//MainTabs");
    }
}