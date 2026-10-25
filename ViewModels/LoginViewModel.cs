using System.Windows.Input;
using Become.Services;

namespace Become.ViewModels;

public class LoginViewModel : BecomeViewModelBase
{
    private readonly IAccountService _accounts;

    private string _email = "", _password = "";
    private string _emailError = "", _passwordError = "", _globalError = "";
    private bool _isBusy, _isPasswordHidden = true;

    public string Email { get => _email; set => Set(ref _email, value); }
    public string Password { get => _password; set => Set(ref _password, value); }

    public string EmailError { get => _emailError; set { if (Set(ref _emailError, value)) OnPropertyChanged(nameof(HasEmailError)); } }
    public string PasswordError { get => _passwordError; set { if (Set(ref _passwordError, value)) OnPropertyChanged(nameof(HasPasswordError)); } }
    public string GlobalError { get => _globalError; set { if (Set(ref _globalError, value)) OnPropertyChanged(nameof(HasGlobalError)); } }

    public bool HasEmailError => !string.IsNullOrEmpty(EmailError);
    public bool HasPasswordError => !string.IsNullOrEmpty(PasswordError);
    public bool HasGlobalError => !string.IsNullOrEmpty(GlobalError);

    public bool IsBusy { get => _isBusy; set { if (Set(ref _isBusy, value)) OnPropertyChanged(nameof(IsNotBusy)); } }
    public bool IsNotBusy => !IsBusy;

    public bool IsPasswordHidden { get => _isPasswordHidden; set => Set(ref _isPasswordHidden, value); }

    public ICommand LoginCommand { get; }
    public ICommand TogglePasswordCommand { get; }
    public ICommand GoToRegisterCommand { get; }

    public LoginViewModel(IAccountService accounts)
    {
        _accounts = accounts;
        LoginCommand = new Command(async () => await LoginAsync());
        TogglePasswordCommand = new Command(() => IsPasswordHidden = !IsPasswordHidden);
        GoToRegisterCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(Views.RegisterPage)));
    }

    private bool Validate()
    {
        EmailError = IsValidEmail(Email) ? "" : "Entre une adresse e-mail valide.";
        PasswordError = string.IsNullOrEmpty(Password) ? "Entre ton mot de passe." : "";
        return !HasEmailError && !HasPasswordError;
    }

    private async Task LoginAsync()
    {
        if (IsBusy) return;
        GlobalError = "";
        if (!Validate()) return;

        try
        {
            IsBusy = true;
            var result = await _accounts.LoginAsync(Email, Password);

            if (!result.Success)
            {
                GlobalError = result.Error ?? "Connexion impossible.";
                return;
            }

            await OnboardingRouter.GoNextAsync();
        }
        catch (Exception)
        {
            GlobalError = "Une erreur est survenue. Réessaie dans un instant.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}