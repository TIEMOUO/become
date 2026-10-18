using System.Windows.Input;
using Become.Helpers;
using Become.Services;

namespace Become.ViewModels;

public class RegisterViewModel : BaseViewModel
{
    private readonly IAuthService _auth;

    private string _firstName = "";
    private string _email = "";
    private string _password = "";
    private bool _acceptTerms;
    private string _firstNameError = "";
    private string _emailError = "";
    private string _passwordError = "";
    private string _termsError = "";
    private string _globalError = "";
    private bool _isBusy;

    public RegisterViewModel(IAuthService auth)
    {
        _auth = auth;
        RegisterCommand = new Command(async () => await RegisterAsync(), () => !IsBusy);
        BackToLoginCommand = new Command(async () => await AppNavigator.BackAsync());
    }

    // ----- Champs -----
    public string FirstName
    {
        get => _firstName;
        set { if (SetProperty(ref _firstName, value)) FirstNameError = ""; }
    }

    public string Email
    {
        get => _email;
        set { if (SetProperty(ref _email, value)) { EmailError = ""; GlobalError = ""; } }
    }

    public string Password
    {
        get => _password;
        set
        {
            if (!SetProperty(ref _password, value)) return;
            PasswordError = "";
            OnPropertyChanged(nameof(PasswordHint));
            OnPropertyChanged(nameof(IsPasswordOk));
            OnPropertyChanged(nameof(IsPasswordWeak));
        }
    }

    public bool AcceptTerms
    {
        get => _acceptTerms;
        set { if (SetProperty(ref _acceptTerms, value)) TermsError = ""; }
    }

    // ----- Erreurs -----
    public string FirstNameError { get => _firstNameError; private set => SetProperty(ref _firstNameError, value); }
    public string EmailError { get => _emailError; private set => SetProperty(ref _emailError, value); }
    public string PasswordError { get => _passwordError; private set => SetProperty(ref _passwordError, value); }

    public string TermsError
    {
        get => _termsError;
        private set { if (SetProperty(ref _termsError, value)) OnPropertyChanged(nameof(HasTermsError)); }
    }
    public bool HasTermsError => !string.IsNullOrEmpty(TermsError);

    public string GlobalError
    {
        get => _globalError;
        private set { if (SetProperty(ref _globalError, value)) OnPropertyChanged(nameof(HasGlobalError)); }
    }
    public bool HasGlobalError => !string.IsNullOrEmpty(GlobalError);

    // ----- Aide mot de passe (feedback en temps réel) -----
    private bool LengthOk => Password.Length >= 8;

    private string Strength
    {
        get
        {
            var categories = new[]
            {
                Password.Any(char.IsLower),
                Password.Any(char.IsUpper),
                Password.Any(char.IsDigit),
                Password.Any(c => !char.IsLetterOrDigit(c))
            }.Count(b => b);

            if (categories >= 3 || Password.Length >= 12) return "fort";
            if (categories == 2) return "moyen";
            return "faible";
        }
    }

    public bool IsPasswordOk => LengthOk && Strength != "faible";
    public bool IsPasswordWeak => LengthOk && Strength == "faible";

    public string PasswordHint => !LengthOk
        ? "8 caractères minimum"
        : $"✓ 8 caractères minimum • Mot de passe {Strength}";

    // ----- Commandes / état -----
    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (!SetProperty(ref _isBusy, value)) return;
            OnPropertyChanged(nameof(ButtonText));
            ((Command)RegisterCommand).ChangeCanExecute();
        }
    }

    public string ButtonText => IsBusy ? "Création en cours…" : "Créer mon compte";

    public ICommand RegisterCommand { get; }
    public ICommand BackToLoginCommand { get; }

    private async Task RegisterAsync()
    {
        GlobalError = "";
        FirstNameError = string.IsNullOrWhiteSpace(FirstName) ? "Indiquez votre prénom." : "";
        EmailError = Validators.Email(Email);
        PasswordError = LengthOk ? "" : "Le mot de passe doit contenir au moins 8 caractères.";
        TermsError = AcceptTerms ? "" : "Acceptez les conditions pour créer votre compte.";

        if (FirstNameError.Length + EmailError.Length + PasswordError.Length + TermsError.Length > 0) return;

        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            GlobalError = "Pas de connexion Internet. Vérifiez votre réseau et réessayez.";
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _auth.RegisterAsync(FirstName.Trim(), Email.Trim(), Password);
            if (result.Success) await AppNavigator.GoHomeAsync();
            else if (result.Code == "email_taken") EmailError = result.Error ?? "";
            else GlobalError = result.Error ?? "Création du compte impossible.";
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
