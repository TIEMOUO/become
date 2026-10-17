namespace Become.Services;

public record AuthResult(bool Success, string? Error = null, string? Code = null, string? FirstName = null);

public interface IAuthService
{
    Task<AuthResult> LoginAsync(string email, string password);
    Task<AuthResult> RegisterAsync(string firstName, string email, string password);
    Task<AuthResult> RequestPasswordResetAsync(string email);
    void Logout();
}

/// <summary>
/// Implémentation de démonstration (aucun serveur).
/// Remplace-la par ton API en gardant l'interface IAuthService.
/// Cas de test :
///  - mot de passe de moins de 8 caractères => identifiants invalides ;
///  - test@become.app à l'inscription => e-mail déjà utilisé ;
///  - la connexion charge des objectifs de démonstration si l'application est vide.
/// </summary>
public class FakeAuthService : IAuthService
{
    private readonly IGoalStore _store;

    public FakeAuthService(IGoalStore store) => _store = store;

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        await Task.Delay(800);

        if (password.Length < 8)
            return new AuthResult(false, "E-mail ou mot de passe incorrect.", "invalid_credentials");

        var name = Preferences.Get("first_name", "");
        if (string.IsNullOrWhiteSpace(name))
        {
            var local = email.Split('@')[0];
            name = char.ToUpper(local[0]) + local[1..];
            Preferences.Set("first_name", name);
        }

        _store.SeedDemoIfEmpty();
        return new AuthResult(true, FirstName: name);
    }

    public async Task<AuthResult> RegisterAsync(string firstName, string email, string password)
    {
        await Task.Delay(900);

        if (email.Equals("test@become.app", StringComparison.OrdinalIgnoreCase))
            return new AuthResult(false, "Un compte existe déjà avec cette adresse e-mail.", "email_taken");

        Preferences.Set("first_name", firstName);
        _store.Clear();   // nouveau compte : écran d'accueil vide
        return new AuthResult(true, FirstName: firstName);
    }

    public async Task<AuthResult> RequestPasswordResetAsync(string email)
    {
        await Task.Delay(800);
        return new AuthResult(true);
    }

    public void Logout() => Preferences.Remove("first_name");
}
