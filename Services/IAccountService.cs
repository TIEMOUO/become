namespace Become.Services;

public record AccountResult(bool Success, string? Error = null);
public record CurrentUser(string FullName, string Email);

public interface IAccountService
{
    Task<CurrentUser?> GetCurrentUserAsync();
    Task<AccountResult> RegisterAsync(string fullName, string email, string password);
    Task<AccountResult> LoginAsync(string email, string password);
    Task LogoutAsync();
    Task<bool> IsAuthenticatedAsync();
}