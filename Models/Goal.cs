using System.Text.Json.Serialization;
using Become.Helpers;

namespace Become.Models;

public enum Frequency { Daily, Weekly, Free }

public class Session
{
    public DateTime Date { get; set; }
    public int Minutes { get; set; }
}

public class Milestone
{
    public string Title { get; set; } = "";
    public int Sessions { get; set; }
}

public static class Categories
{
    public static readonly string[] Names =
        { "Apprentissage", "Santé", "Bien-être", "Travail", "Créativité", "Autre" };

    public static string IconFor(string category) => category switch
    {
        "Apprentissage" => "🎓",
        "Santé" => "💪",
        "Bien-être" => "🌸",
        "Travail" => "💼",
        "Créativité" => "🎨",
        _ => "⭐"
    };
}

public class Goal
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Title { get; set; } = "";
    public string Why { get; set; } = "";
    public string Category { get; set; } = "Autre";
    public string? CustomIcon { get; set; }
    public Frequency Frequency { get; set; } = Frequency.Daily;
    public List<DayOfWeek> WeekDays { get; set; } = new();
    public int SessionMinutes { get; set; } = 15;
    public DateTime StartDate { get; set; } = DateTime.Today;
    public DateTime TargetDate { get; set; } = DateTime.Today.AddDays(30);
    public bool ReminderEnabled { get; set; }
    public TimeSpan ReminderTime { get; set; } = new(18, 0, 0);
    public int TotalSessions { get; set; }
    public List<Session> Sessions { get; set; } = new();
    public List<Milestone> Milestones { get; set; } = new();

    // ----- Valeurs dérivées (non stockées) -----
    [JsonIgnore] public string Icon => CustomIcon ?? Categories.IconFor(Category);
    [JsonIgnore] public int Done => Sessions.Count;
    [JsonIgnore] public int Remaining => Math.Max(0, TotalSessions - Done);
    [JsonIgnore] public double Progress => TotalSessions == 0 ? 0 : Math.Min(1.0, (double)Done / TotalSessions);
    [JsonIgnore] public string ProgressText => $"{Done} / {TotalSessions} séances";
    [JsonIgnore] public string PercentText => $"{Math.Round(Progress * 100, MidpointRounding.AwayFromZero)}%";

    [JsonIgnore] public bool IsAchieved => TotalSessions > 0 && Done >= TotalSessions;
    [JsonIgnore] public bool IsFinished => IsAchieved || TargetDate.Date < DateTime.Today;
    [JsonIgnore] public string StatusText => IsAchieved ? "Atteint" : IsFinished ? "Non atteint" : "En cours";

    [JsonIgnore]
    public string FrequencyLabel => Frequency switch
    {
        Frequency.Daily => "Chaque jour",
        Frequency.Weekly => WeekDays.Count >= 7 ? "Chaque jour" : $"{WeekDays.Count} fois / semaine",
        _ => "Libre"
    };

    [JsonIgnore] public string Subtitle => $"{FrequencyLabel} • {Fmt.Short(TargetDate)}";
    [JsonIgnore] public string FinishedSubtitle => $"{ProgressText} • {Fmt.Short(TargetDate)}";
    [JsonIgnore] public string DetailSubtitle => $"{FrequencyLabel} • Objectif au {Fmt.Long(TargetDate)}";

    // ----- Planning -----
    public bool IsScheduledOn(DateTime day) => Frequency switch
    {
        Frequency.Daily => true,
        Frequency.Weekly => WeekDays.Contains(day.DayOfWeek),
        _ => true   // libre : disponible tous les jours
    };

    public bool IsDoneOn(DateTime day) => Sessions.Any(s => s.Date.Date == day.Date);

    public IEnumerable<DateTime> DoneDays => Sessions.Select(s => s.Date.Date).Distinct();

    /// <summary>Nombre de séances prévues entre deux dates (incluses).</summary>
    public static int CountScheduled(Frequency frequency, IEnumerable<DayOfWeek> weekDays, DateTime start, DateTime end)
    {
        if (end.Date < start.Date) return 0;
        var days = (end.Date - start.Date).Days + 1;
        if (frequency == Frequency.Daily) return days;
        if (frequency == Frequency.Free) return Math.Max(1, days / 3);

        var set = weekDays.ToHashSet();
        var count = 0;
        for (var d = start.Date; d <= end.Date; d = d.AddDays(1))
            if (set.Contains(d.DayOfWeek)) count++;
        return count;
    }

    /// <summary>Trois jalons : démarrage, mi-parcours, objectif atteint.</summary>
    public static List<Milestone> BuildMilestones(int total)
    {
        if (total <= 0) return new();
        var first = Math.Max(1, (int)Math.Round(total * 0.17, MidpointRounding.AwayFromZero));
        var mid = Math.Max(first + 1, (int)Math.Round(total * 0.5, MidpointRounding.AwayFromZero));
        if (mid >= total) return new() { new() { Title = "Ancrer votre habitude", Sessions = total } };
        if (first >= mid) first = mid - 1;
        if (first < 1) return new()
        {
            new() { Title = "Passer la mi-parcours", Sessions = mid },
            new() { Title = "Ancrer votre habitude", Sessions = total }
        };
        return new()
        {
            new() { Title = "Prendre le rythme", Sessions = first },
            new() { Title = "Passer la mi-parcours", Sessions = mid },
            new() { Title = "Ancrer votre habitude", Sessions = total }
        };
    }
}

/// <summary>Résultat de la validation (ou annulation) d'une séance du jour.</summary>
public record ToggleResult(
    bool Validated,
    int StreakBefore,
    int StreakAfter,
    Milestone? ReachedMilestone,
    bool GoalAchieved);
