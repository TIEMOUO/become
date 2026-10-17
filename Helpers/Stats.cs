namespace Become.Helpers;

public static class Stats
{
    /// <summary>
    /// Série en cours : jours consécutifs avec au moins une séance.
    /// Si rien n'est encore validé aujourd'hui, la série d'hier reste vivante.
    /// </summary>
    public static int CurrentStreak(IEnumerable<DateTime> days, DateTime today)
    {
        var set = days.Select(d => d.Date).ToHashSet();
        var d = set.Contains(today.Date) ? today.Date : today.Date.AddDays(-1);
        var n = 0;
        while (set.Contains(d))
        {
            n++;
            d = d.AddDays(-1);
        }
        return n;
    }

    public static int BestStreak(IEnumerable<DateTime> days)
    {
        var sorted = days.Select(d => d.Date).Distinct().OrderBy(d => d).ToList();
        int best = 0, run = 0;
        DateTime? prev = null;
        foreach (var d in sorted)
        {
            run = prev.HasValue && (d - prev.Value).Days == 1 ? run + 1 : 1;
            best = Math.Max(best, run);
            prev = d;
        }
        return best;
    }
}
