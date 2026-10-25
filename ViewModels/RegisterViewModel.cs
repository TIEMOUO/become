using System.Windows.Input;
using Become.Services;

namespace Become.ViewModels;

public class RegisterViewModel : BecomeViewModelBase
{
    private readonly IAccountService _accounts;

    private string _fullName = "", _email = "", _password = "", _confirmPassword = "";
    private string _fullNameError = "", _emailError = "", _passwordError = "", _confirmPasswordError = "", _globalError = "";
    private bool _isBusy, _isPasswordHidden = true;

    public string FullName { get => _fullName; set => Set(ref _fullName, value); }
    public string Email { get => _email; set => Set(ref _email, value); }
    public string Password { get => _password; set => Set(ref _password, value); }
    public string ConfirmPassword { get => _confirmPassword; set => Set(ref _confirmPassword, value); }

    public string FullNameError { get => _fullNameError; set { if (Set(ref _fullNameError, value)) OnPropertyChanged(nameof(HasFullNameError)); } }
    public string EmailError { get => _emailError; set { if (Set(ref _emailError, value)) OnPropertyChanged(nameof(HasEmailError)); } }
    public string PasswordError { get => _passwordError; set { if (Set(ref _passwordError, value)) OnPropertyChanged(nameof(HasPasswordError)); } }
    public string ConfirmPasswordError { get => _confirmPasswordError; set { if (Set(ref _confirmPasswordError, value)) OnPropertyChanged(nameof(HasConfirmPasswordError)); } }
    public string GlobalError { get => _globalError; set { if (Set(ref _globalError, value)) OnPropertyChanged(nameof(HasGlobalError)); } }

    public bool HasFullNameError => !string.IsNullOrEmpty(FullNameError);
    public bool HasEmailError => !string.IsNullOrEmpty(EmailError);
    public bool HasPasswordError => !string.IsNullOrEmpty(PasswordError);
    public bool HasConfirmPasswordError => !string.IsNullOrEmpty(ConfirmPasswordError);
    public bool HasGlobalError => !string.IsNullOrEmpty(GlobalError);

    public bool IsBusy { get => _isBusy; set { if (Set(ref _isBusy, value)) OnPropertyChanged(nameof(IsNotBusy)); } }
    public bool IsNotBusy => !IsBusy;
    public bool IsPasswordHidden { get => _isPasswordHidden; set => Set(ref _isPasswordHidden, value); }

    public ICommand RegisterCommand { get; }
    public ICommand TogglePasswordCommand { get; }
    public ICommand GoToLoginCommand { get; }

    public RegisterViewModel(IAccountService accounts)
    {
        _accounts = accounts;
        RegisterCommand = new Command(async () => await RegisterAsync());
        TogglePasswordCommand = new Command(() => IsPasswordHidden = !IsPasswordHidden);
        GoToLoginCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(Views.LoginPage)));
    }

    private bool Validate()
    {
        FullNameError = string.IsNullOrWhiteSpace(FullName) ? "Entre ton nom." : "";
        EmailError = IsValidEmail(Email) ? "" : "Entre une adresse e-mail valide.";
        PasswordError = Password.Length < 8 ? "Le mot de passe doit contenir au moins 8 caractères." : "";
        ConfirmPasswordError = Password != ConfirmPassword ? "Les mots de passe ne correspondent pas." : "";

        return !HasFullNameError && !HasEmailError && !HasPasswordError && !HasConfirmPasswordError;
    }

    private async Task RegisterAsync()
    {
        if (IsBusy) return;
        GlobalError = "";
        if (!Validate()) return;

        try
        {
            IsBusy = true;
            var result = await _accounts.RegisterAsync(FullName, Email, Password);

            if (!result.Success)
            {
                GlobalError = result.Error ?? "Impossible de créer le compte.";
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