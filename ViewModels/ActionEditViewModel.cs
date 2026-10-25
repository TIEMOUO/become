using System.Windows.Input;
using Become.Services;

namespace Become.ViewModels;

public class ActionEditViewModel : BecomeViewModelBase, IQueryAttributable
{
    private readonly IBecomeStore _store;
    private Guid _actionId;
    private string _title = "", _description = "", _minutes = "", _error = "";

    public string Title { get => _title; set => Set(ref _title, value); }
    public string Description { get => _description; set => Set(ref _description, value); }
    public string Minutes { get => _minutes; set => Set(ref _minutes, value); }

    public string Error
    {
        get => _error;
        set { if (Set(ref _error, value)) OnPropertyChanged(nameof(HasError)); }
    }
    public bool HasError => !string.IsNullOrEmpty(Error);

    public ICommand SaveCommand { get; }

    public ActionEditViewModel(IBecomeStore store)
    {
        _store = store;
        SaveCommand = new Command(async () => await SaveAsync());
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var value) && Guid.TryParse(value?.ToString(), out var id))
        {
            _actionId = id;
            _ = LoadAsync();
        }
    }

    private async Task LoadAsync()
    {
        var d = await _store.GetAsync();
        var a = d.Actions.FirstOrDefault(x => x.Id == _actionId);
        if (a is null) return;

        Title = a.Title;
        Description = a.Description;
        Minutes = a.EstimatedMinutes?.ToString() ?? "";
    }

    private async Task SaveAsync()
    {
        Error = "";
        if (string.IsNullOrWhiteSpace(Title))
        {
            Error = "Donne un titre à cette action.";
            return;
        }

        int? minutes = null;
        if (!string.IsNullOrWhiteSpace(Minutes))
        {
            if (!int.TryParse(Minutes.Trim(), out var m) || m < 1 || m > 240)
            {
                Error = "La durée doit être un nombre de minutes entre 1 et 240.";
                return;
            }
            minutes = m;
        }

        var d = await _store.GetAsync();
        var a = d.Actions.FirstOrDefault(x => x.Id == _actionId);
        if (a is null) return;

        a.Title = Title.Trim();
        a.Description = (Description ?? "").Trim();
        a.EstimatedMinutes = minutes;

        await _store.SaveAsync();
        await Shell.Current.GoToAsync("..");
    }
}