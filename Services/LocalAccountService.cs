using System.Security.Cryptography;
using System.Text.Json;

namespace Become.Services;

internal record StoredAccount(string FullName, string Email, string Salt, string Hash, DateTime CreatedAt);

public class LocalAccountService : IAccountService
{
    private const string AccountsKey = "become_accounts";
    private const string SessionKey = "become_session_email";
    private const int Iterations = 100_000;

    public async Task<AccountResult> RegisterAsync(string fullName, string email, string password)
    {
        var accounts = await LoadAsync();
        var normalized = email.Trim().ToLowerInvariant();

        if (accounts.Any(a => a.Email == normalized))
            return new AccountResult(false, "Un compte existe déjà avec cette adresse e-mail.");

        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = HashPassword(password, salt);

        accounts.Add(new StoredAccount(
            fullName.Trim(), normalized,
            Convert.ToBase64String(salt), Convert.ToBase64String(hash),
            DateTime.UtcNow));

        await SaveAsync(accounts);
        await SecureStorage.Default.SetAsync(SessionKey, normalized);
        return new AccountResult(true);
    }

    public async Task<AccountResult> LoginAsync(string email, string password)
    {
        var accounts = await LoadAsync();
        var normalized = email.Trim().ToLowerInvariant();
        var account = accounts.FirstOrDefault(a => a.Email == normalized);

        const string invalid = "E-mail ou mot de passe incorrect.";
        if (account is null) return new AccountResult(false, invalid);

        var expected = Convert.FromBase64String(account.Hash);
        var actual = HashPassword(password, Convert.FromBase64String(account.Salt));
        if (!CryptographicOperations.FixedTimeEquals(expected, actual))
            return new AccountResult(false, invalid);

        await SecureStorage.Default.SetAsync(SessionKey, normalized);
        return new AccountResult(true);
    }

    public Task LogoutAsync()
    {
        SecureStorage.Default.Remove(SessionKey);
        return Task.CompletedTask;
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        try { return await SecureStorage.Default.GetAsync(SessionKey) is not null; }
        catch { return false; }
    }

    private static byte[] HashPassword(string password, byte[] salt) =>
        Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, 32);
    public async Task<CurrentUser?> GetCurrentUserAsync()
    {
        try
        {
            var email = await SecureStorage.Default.GetAsync(SessionKey);
            if (email is null) return null;

            var account = (await LoadAsync()).FirstOrDefault(a => a.Email == email);
            return account is null ? null : new CurrentUser(account.FullName, account.Email);
        }
        catch
        {
            return null;
        }
    }
    private static async Task<List<StoredAccount>> LoadAsync()
    {
        try
        {
            var json = await SecureStorage.Default.GetAsync(AccountsKey);
            return string.IsNullOrEmpty(json)
                ? new List<StoredAccount>()
                : JsonSerializer.Deserialize<List<StoredAccount>>(json) ?? new List<StoredAccount>();
        }
        catch
        {
            return new List<StoredAccount>();
        }
    }

    private static Task SaveAsync(List<StoredAccount> accounts) =>
        SecureStorage.Default.SetAsync(AccountsKey, JsonSerializer.Serialize(accounts));
}