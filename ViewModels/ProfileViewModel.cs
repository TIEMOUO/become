using System.Windows.Input;
using Become.Services;

namespace Become.ViewModels;

public class ProfileViewModel : BecomeViewModelBase
{
    private readonly IBecomeStore _store;
    private readonly IAccountService _accounts;
    private string _name = "", _email = "", _initial = "", _visionText = "";

    public string Name { get => _name; set => Set(ref _name, value); }
    public string Email { get => _email; set => Set(ref _email, value); }
    public string Initial { get => _initial; set => Set(ref _initial, value); }
    public string VisionText { get => _visionText; set => Set(ref _visionText, value); }

    public ICommand EditVisionCommand { get; }
    public ICommand EditPrioritiesCommand { get; }
    public ICommand LogoutCommand { get; }

    public ProfileViewModel(IBecomeStore store, IAccountService accounts)
    {
        _store = store;
        _accounts = accounts;

        EditVisionCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(Views.VisionResultPage)));
        EditPrioritiesCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(Views.PrioritiesPage)));
        LogoutCommand = new Command(async () => await LogoutAsync());
    }

    public async Task LoadAsync()
    {
        var user = await _accounts.GetCurrentUserAsync();
        var d = await _store.GetAsync();

        Name = user?.FullName ?? "";
        Email = user?.Email ?? "";
        Initial = string.IsNullOrEmpty(Name) ? "?" : Name.Trim()[0].ToString().ToUpperInvariant();
        VisionText = d.Vision?.VisionText ?? "";
    }

    private async Task LogoutAsync()
    {
        await _accounts.LogoutAsync();
        await Shell.Current.GoToAsync("//WelcomePage");
    }
}