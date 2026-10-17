using System.Windows.Input;
using Become.Helpers;
using Become.Services;

namespace Become.ViewModels;

public class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _auth;

    private string _email = "";
    private string _password = "";
    private string _emailError = "";
    private string _passwordError = "";
    private string _globalError = "";
    private bool _isBusy;

    public LoginViewModel(IAuthService auth)
    {
        _auth = auth;
        LoginCommand = new Command(async () => await LoginAsync(), () => !IsBusy);
        RegisterCommand = new Command(async () => await AppNavigator.OpenRegisterAsync());
        ForgotPasswordCommand = new Command(async () => await AppNavigator.OpenForgotPasswordAsync());
        AppleCommand = new Command(async () => await Shell.Current.DisplayAlert("Bientôt disponible", "La connexion avec Apple sera ajoutée prochainement.", "OK"));
    }

    public string Email
    {
        get => _email;
        set { if (SetProperty(ref _email, value)) { EmailError = ""; GlobalError = ""; } }
    }

    public string Password
    {
        get => _password;
        set { if (SetProperty(ref _password, value)) { PasswordError = ""; GlobalError = ""; } }
    }

    public string EmailError { get => _emailError; private set => SetProperty(ref _emailError, value); }
    public string PasswordError { get => _passwordError; private set => SetProperty(ref _passwordError, value); }

    public string GlobalError
    {
        get => _globalError;
        private set { if (SetProperty(ref _globalError, value)) OnPropertyChanged(nameof(HasGlobalError)); }
    }
    public bool HasGlobalError => !string.IsNullOrEmpty(GlobalError);

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (!SetProperty(ref _isBusy, value)) return;
            OnPropertyChanged(nameof(ButtonText));
            ((Command)LoginCommand).ChangeCanExecute();
        }
    }

    public string ButtonText => IsBusy ? "Connexion en cours…" : $"{Icons.Arrow}  Se connecter";

    public ICommand LoginCommand { get; }
    public ICommand RegisterCommand { get; }
    public ICommand ForgotPasswordCommand { get; }
    public ICommand AppleCommand { get; }

    private async Task LoginAsync()
    {
        GlobalError = "";
        EmailError = Validators.Email(Email);
        PasswordError = string.IsNullOrWhiteSpace(Password) ? "Saisissez votre mot de passe." : "";
        if (EmailError.Length > 0 || PasswordError.Length > 0) return;

        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            GlobalError = "Pas de connexion Internet. Vérifiez votre réseau et réessayez.";
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _auth.LoginAsync(Email.Trim(), Password);
            if (result.Success) await AppNavigator.GoHomeAsync();
            else GlobalError = result.Error ?? "Connexion impossible.";
        }
        catch
        {
            GlobalError = "Un problème est survenu. Réessayez dans un instant.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
