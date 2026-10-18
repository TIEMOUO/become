using System.Windows.Input;
using Become.Helpers;
using Become.Services;

namespace Become.ViewModels;

public class ForgotPasswordViewModel : BaseViewModel
{
    private readonly IAuthService _auth;

    private string _email = "";
    private string _emailError = "";
    private string _globalError = "";
    private bool _isBusy;
    private bool _isSent;

    public ForgotPasswordViewModel(IAuthService auth)
    {
        _auth = auth;
        SendCommand = new Command(async () => await SendAsync(), () => !IsBusy);
        BackCommand = new Command(async () => await AppNavigator.BackAsync());
    }

    public string Email
    {
        get => _email;
        set { if (SetProperty(ref _email, value)) { EmailError = ""; GlobalError = ""; } }
    }

    public string EmailError { get => _emailError; private set => SetProperty(ref _emailError, value); }

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
            ((Command)SendCommand).ChangeCanExecute();
        }
    }

    /// <summary>Vrai quand le lien a été envoyé : l'écran affiche la confirmation.</summary>
    public bool IsSent
    {
        get => _isSent;
        private set
        {
            if (!SetProperty(ref _isSent, value)) return;
            OnPropertyChanged(nameof(IsNotSent));
            OnPropertyChanged(nameof(HeaderTitle));
            OnPropertyChanged(nameof(HeaderSubtitle));
        }
    }
    public bool IsNotSent => !IsSent;

    // En-tête : change quand le lien a été envoyé
    public string HeaderTitle => IsSent ? "Vérifiez votre boîte mail" : "Mot de passe oublié ?";
    public string HeaderSubtitle => IsSent
        ? "Nous venons de vous envoyer un lien de réinitialisation."
        : "Indiquez l'adresse e-mail associée à votre compte. Nous vous enverrons un lien pour le réinitialiser.";

    public string SentMessage => $"Si un compte existe pour {Email.Trim()}, vous recevrez un lien de réinitialisation dans quelques instants.";

    public string ButtonText => IsBusy ? "Envoi en cours…" : "Envoyer le lien de réinitialisation";

    public ICommand SendCommand { get; }
    public ICommand BackCommand { get; }

    private async Task SendAsync()
    {
        GlobalError = "";
        EmailError = Validators.Email(Email);
        if (EmailError.Length > 0) return;

        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            GlobalError = "Pas de connexion Internet. Vérifiez votre réseau et réessayez.";
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _auth.RequestPasswordResetAsync(Email.Trim());
            if (result.Success)
            {
                OnPropertyChanged(nameof(SentMessage));
                IsSent = true;
            }
            else GlobalError = result.Error ?? "Envoi impossible.";
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
