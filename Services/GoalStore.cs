using System.Text.Json;
using Become.Helpers;
using Become.Models;

namespace Become.Services;

public interface IGoalStore
{
    /// <summary>Déclenché après toute modification (ajout, suppression, validation).</summary>
    event EventHandler? Changed;

    IReadOnlyList<Goal> Goals { get; }
    Goal? Get(string id);
    void Add(Goal goal);
    void Remove(string id);

    /// <summary>Objectifs actifs prévus aujourd'hui.</summary>
    IReadOnlyList<Goal> DueToday();

    /// <summary>Série globale (au moins une séance par jour, tous objectifs confondus).</summary>
    int GlobalStreak();

    /// <summary>Valide la séance du jour, ou l'annule si elle est déjà validée.</summary>
    ToggleResult ToggleToday(Goal goal);

    void SeedDemoIfEmpty();
    void Clear();

    /// <summary>Enregistre les modifications faites directement sur un objectif (ex. rappel).</summary>
    void Save();
}

/// <summary>
/// Stockage « offline-first » : tout est écrit dans un petit fichier JSON local.
/// Aucune requête réseau n'est nécessaire pour utiliser l'application au quotidien.
/// </summary>
public class JsonGoalStore : IGoalStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = false };

    private readonly string _path = Path.Combine(FileSystem.AppDataDirectory, "goals.json");
    private List<Goal> _goals = new();

    public JsonGoalStore() => Load();

    public event EventHandler? Changed;

    public IReadOnlyList<Goal> Goals => _goals;

    public Goal? Get(string id) => _goals.FirstOrDefault(g => g.Id == id);

    public void Add(Goal goal)
    {
        _goals.Add(goal);
        Commit();
    }

    public void Remove(string id)
    {
        _goals.RemoveAll(g => g.Id == id);
        Commit();
    }

    public void Save() => Commit();

    public void Clear()
    {
        _goals.Clear();
        Commit();
    }

    public IReadOnlyList<Goal> DueToday()
    {
        var today = DateTime.Today;
        return _goals
            .Where(g => !g.IsFinished && g.StartDate.Date <= today && g.IsScheduledOn(today))
            .ToList();
    }

    public int GlobalStreak()
        => Stats.CurrentStreak(_goals.SelectMany(g => g.DoneDays), DateTime.Today);

    public ToggleResult ToggleToday(Goal goal)
    {
        var today = DateTime.Today;
        var streakBefore = GlobalStreak();
        var doneBefore = goal.Done;

        if (goal.IsDoneOn(today))
        {
            goal.Sessions.RemoveAll(s => s.Date.Date == today);
            Commit();
            return new ToggleResult(false, streakBefore, GlobalStreak(), null, false);
        }

        goal.Sessions.Add(new Session { Date = today, Minutes = goal.SessionMinutes });
        Commit();

        var reached = goal.Milestones.FirstOrDefault(m => doneBefore < m.Sessions && m.Sessions <= goal.Done);
        var achieved = doneBefore < goal.TotalSessions && goal.Done >= goal.TotalSessions;
        return new ToggleResult(true, streakBefore, GlobalStreak(), reached, achieved);
    }

    // ------------------------------------------------------------------
    // Persistance
    // ------------------------------------------------------------------
    private void Load()
    {
        try
        {
            if (File.Exists(_path))
                _goals = JsonSerializer.Deserialize<List<Goal>>(File.ReadAllText(_path), Options) ?? new();
        }
        catch
        {
            _goals = new();   // fichier illisible : on repart proprement
        }
    }

    private void Commit()
    {
        try { File.WriteAllText(_path, JsonSerializer.Serialize(_goals, Options)); }
        catch { /* stockage indisponible : l'état en mémoire reste valable */ }
        Changed?.Invoke(this, EventArgs.Empty);
    }

    // ------------------------------------------------------------------
    // Données de démonstration (mêmes objectifs que les maquettes)
    // ------------------------------------------------------------------
    public void SeedDemoIfEmpty()
    {
        if (_goals.Count > 0) return;

        var today = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);
        var prevStart = monthStart.AddMonths(-1);
        var prevEnd = monthStart.AddDays(-1);

        // --- Objectifs en cours ---
        var lire = Make("Lire 20 minutes", "Retrouver le plaisir de lire et prendre un moment pour moi.",
            "Apprentissage", "📖", Frequency.Daily, null, 20, today.AddDays(-45), monthEnd, total: 30);
        PlaceBackward(lire, 18, includeToday: false, gapAfter: 7);

        var mediter = Make("Méditer 10 minutes", "Être plus calme et plus présent au quotidien.",
            "Bien-être", "🌸", Frequency.Weekly, Weekdays(1, 2, 3, 4, 5), 10, today.AddDays(-45), monthEnd, total: 20);
        PlaceBackward(mediter, 12, includeToday: true);

        var bouger = Make("Bouger 30 minutes", "Prendre soin de mon corps et de mon énergie.",
            "Santé", "👣", Frequency.Weekly, Weekdays(1, 2, 4, 6), 30, today.AddDays(-45), monthEnd, total: 16);
        PlaceBackward(bouger, 8, includeToday: false);

        // --- Objectifs terminés (mois précédent) ---
        var ecrire = Make("Écrire chaque matin", "Mettre mes idées au clair.",
            "Créativité", "✍️", Frequency.Daily, null, 15, prevStart, prevStart.AddDays(19),
            total: Goal.CountScheduled(Frequency.Daily, Array.Empty<DayOfWeek>(), prevStart, prevStart.AddDays(19)));
        PlaceForward(ecrire, ecrire.TotalSessions);

        var ecranStart = prevStart.AddDays(19);
        var ecran = Make("Moins d'écran le soir", "Mieux dormir.",
            "Bien-être", "🌙", Frequency.Daily, null, 0, ecranStart, prevEnd,
            total: Goal.CountScheduled(Frequency.Daily, Array.Empty<DayOfWeek>(), ecranStart, prevEnd));
        PlaceForward(ecran, ecran.TotalSessions);

        var espagnolDays = Weekdays(1, 3);
        var espagnol = Make("Pratiquer l'espagnol", "Me sentir à l'aise pour mon prochain voyage.",
            "Apprentissage", "🎓", Frequency.Weekly, espagnolDays, 20, prevStart, prevEnd,
            total: Goal.CountScheduled(Frequency.Weekly, espagnolDays, prevStart, prevEnd));
        PlaceForward(espagnol, espagnol.TotalSessions / 2);

        _goals = new List<Goal> { lire, mediter, bouger, ecrire, ecran, espagnol };
        Commit();
    }

    private static List<DayOfWeek> Weekdays(params int[] days) => days.Select(d => (DayOfWeek)(d % 7)).ToList();

    private static Goal Make(string title, string why, string category, string icon, Frequency frequency,
        List<DayOfWeek>? days, int minutes, DateTime start, DateTime target, int total)
        => new()
        {
            Title = title,
            Why = why,
            Category = category,
            CustomIcon = icon,
            Frequency = frequency,
            WeekDays = days ?? new(),
            SessionMinutes = minutes,
            StartDate = start,
            TargetDate = target,
            TotalSessions = total,
            Milestones = Goal.BuildMilestones(total),
            ReminderEnabled = true
        };

    private static void PlaceBackward(Goal g, int count, bool includeToday, int gapAfter = -1)
    {
        var d = includeToday ? DateTime.Today : DateTime.Today.AddDays(-1);
        var placed = 0;
        var gapDone = false;
        while (placed < count && d >= g.StartDate.Date)
        {
            if (g.IsScheduledOn(d))
            {
                if (gapAfter > 0 && placed == gapAfter && !gapDone)
                {
                    gapDone = true;            // un jour manqué pour que la série reste réaliste
                    d = d.AddDays(-1);
                    continue;
                }
                g.Sessions.Add(new Session { Date = d, Minutes = g.SessionMinutes + (placed % 2 == 0 ? 0 : 5) });
                placed++;
            }
            d = d.AddDays(-1);
        }
        g.Sessions.Sort((a, b) => a.Date.CompareTo(b.Date));
    }

    private static void PlaceForward(Goal g, int count)
    {
        var d = g.StartDate.Date;
        var placed = 0;
        while (placed < count && d <= g.TargetDate.Date)
        {
            if (g.IsScheduledOn(d))
            {
                g.Sessions.Add(new Session { Date = d, Minutes = g.SessionMinutes });
                placed++;
            }
            d = d.AddDays(1);
        }
    }
}
