using System.Globalization;

namespace Become.Domain;

public static class LifeDomains
{
    public const string Career = "Études et carrière";
    public const string Finance = "Finances";
    public const string Health = "Santé et bien-être";
    public const string Relations = "Relations";
    public const string Growth = "Développement personnel";
    public const string Spirituality = "Spiritualité";

    public static readonly string[] Standard = { Career, Finance, Health, Relations, Growth, Spirituality };
}

public enum GoalStatus { Active, Achieved, Archived }

public class VisionProfile
{
    public string WhoIWantToBecome { get; set; } = "";
    public string ImportantAreas { get; set; } = "";
    public string ValuesAndAspirations { get; set; } = "";
    public string LifeIWantToBuild { get; set; } = "";
    public string VisionText { get; set; } = "";
    public bool IsValidated { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class VisionVersion
{
    public string Text { get; set; } = "";
    public DateTime Date { get; set; } = DateTime.UtcNow;
}

public class PrioritiesProfile
{
    public List<string> Domains { get; set; } = new();
    public int MinutesPerDay { get; set; } = 30;
    public string Constraints { get; set; } = "";
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class LifeGoal
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Domain { get; set; } = "";
    public string VisionLink { get; set; } = "";
    public string ExpectedResult { get; set; } = "";
    public GoalStatus Status { get; set; } = GoalStatus.Active;
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class DailyAction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SeriesId { get; set; } = Guid.NewGuid();
    public Guid GoalId { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime ScheduledDate { get; set; } = DateTime.Today;
    public bool IsDone { get; set; }
    public DateTime? DoneAt { get; set; }
    public int? EstimatedMinutes { get; set; }
    public string Frequency { get; set; } = "Quotidienne";
}

public class UserData
{
    public VisionProfile? Vision { get; set; }
    public List<VisionVersion> VisionHistory { get; set; } = new();
    public PrioritiesProfile? Priorities { get; set; }
    public List<LifeGoal> Goals { get; set; } = new();
    public List<DailyAction> Actions { get; set; } = new();
}

public static class FrenchDates
{
    private static readonly CultureInfo Fr = new("fr-FR");

    public static string Long(DateTime date)
    {
        var s = date.ToString("dddd d MMMM", Fr);
        return char.ToUpper(s[0], Fr) + s[1..];
    }
}