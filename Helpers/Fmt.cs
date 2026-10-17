using System.Globalization;

namespace Become.Helpers;

/// <summary>Formats de dates en français, utilisés dans toute l'application.</summary>
public static class Fmt
{
    public static readonly CultureInfo Fr = new("fr-FR");

    /// <summary>31 oct.</summary>
    public static string Short(DateTime d) => d.ToString("d MMM", Fr);

    /// <summary>31 octobre 2026</summary>
    public static string Long(DateTime d) => d.ToString("d MMMM yyyy", Fr);

    /// <summary>Hier, 5 octobre / Dimanche 4 octobre</summary>
    public static string Relative(DateTime d)
    {
        var today = DateTime.Today;
        if (d.Date == today) return "Aujourd'hui, " + d.ToString("d MMMM", Fr);
        if (d.Date == today.AddDays(-1)) return "Hier, " + d.ToString("d MMMM", Fr);
        return Cap(d.ToString("dddd d MMMM", Fr));
    }

    /// <summary>Septembre 2026</summary>
    public static string Month(DateTime d) => Cap(d.ToString("MMMM yyyy", Fr));

    public static string Cap(string s) => string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0], Fr) + s[1..];

    public static string DayName(DayOfWeek d) => Fr.DateTimeFormat.GetDayName(d);

    /// <summary>L M M J V S D (lundi en premier)</summary>
    public static readonly (DayOfWeek Day, string Letter)[] WeekDays =
    {
        (DayOfWeek.Monday, "L"), (DayOfWeek.Tuesday, "M"), (DayOfWeek.Wednesday, "M"),
        (DayOfWeek.Thursday, "J"), (DayOfWeek.Friday, "V"), (DayOfWeek.Saturday, "S"),
        (DayOfWeek.Sunday, "D")
    };

    public static string Letter(DayOfWeek d) => WeekDays.First(w => w.Day == d).Letter;
}
