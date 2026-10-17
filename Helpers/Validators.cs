using System.Text.RegularExpressions;

namespace Become.Helpers;

public static class Validators
{
    private static readonly Regex EmailRegex =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    /// <summary>Retourne un message d'erreur, ou une chaîne vide si l'e-mail est valide.</summary>
    public static string Email(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "Saisissez votre adresse e-mail.";
        return EmailRegex.IsMatch(value.Trim()) ? "" : "Cette adresse e-mail semble incorrecte.";
    }
}
