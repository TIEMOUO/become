using System.Windows.Input;
using Become.Services;

namespace Become.ViewModels;

public class GoalEditViewModel : BecomeViewModelBase, IQueryAttributable
{
    private readonly IBecomeStore _store;
    private Guid _goalId;
    private string _title = "", _description = "", _expectedResult = "", _error = "";
    private bool _hasDueDate;
    private DateTime _dueDate = DateTime.Today.AddMonths(1);

    public string Title { get => _title; set => Set(ref _title, value); }
    public string Description { get => _description; set => Set(ref _description, value); }
    public string ExpectedResult { get => _expectedResult; set => Set(ref _expectedResult, value); }
    public bool HasDueDate { get => _hasDueDate; set => Set(ref _hasDueDate, value); }
    public DateTime DueDate { get => _dueDate; set => Set(ref _dueDate, value); }

    public string Error
    {
        get => _error;
        set { if (Set(ref _error, value)) OnPropertyChanged(nameof(HasError)); }
    }
    public bool HasError => !string.IsNullOrEmpty(Error);

    public ICommand SaveCommand { get; }

    public GoalEditViewModel(IBecomeStore store)
    {
        _store = store;
        SaveCommand = new Command(async () => await SaveAsync());
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var value) && Guid.TryParse(value?.ToString(), out var id))
        {
            _goalId = id;
            _ = LoadAsync();
        }
    }

    private async Task LoadAsync()
    {
        var d = await _store.GetAsync();
        var g = d.Goals.FirstOrDefault(x => x.Id == _goalId);
        if (g is null) return;

        Title = g.Title;
        Description = g.Description;
        ExpectedResult = g.ExpectedResult;
        HasDueDate = g.DueDate is not null;
        if (g.DueDate is DateTime due) DueDate = due;
    }

    private async Task SaveAsync()
    {
        Error = "";
        if ((Title ?? "").Trim().Length < 3)
        {
            Error = "Donne un titre clair à ton objectif.";
            return;
        }

        var d = await _store.GetAsync();
        var g = d.Goals.FirstOrDefault(x => x.Id == _goalId);
        if (g is null) return;

        g.Title = Title.Trim();
        g.Description = (Description ?? "").Trim();
        g.ExpectedResult = (ExpectedResult ?? "").Trim();
        g.DueDate = HasDueDate ? DueDate.Date : null;

        await _store.SaveAsync();
        await Shell.Current.GoToAsync("..");
    }
}