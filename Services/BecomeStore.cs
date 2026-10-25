using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Become.Domain;

namespace Become.Services;

public interface IBecomeStore
{
    Task<UserData> GetAsync();
    Task SaveAsync();
    Task<(bool Ok, string? Error)> AddGoalAsync(LifeGoal goal);
    Task<(bool Ok, string? Error)> AddActionAsync(DailyAction action);
    Task EnsureTodayActionsAsync();
}

public class BecomeStore : IBecomeStore
{
    public const int MaxActiveGoals = 3;
    public const int MaxActionsPerDay = 5;
    private const string SessionKey = "become_session_email";

    private UserData? _data;
    private string? _email;

    public async Task<UserData> GetAsync()
    {
        var email = await SecureStorage.Default.GetAsync(SessionKey);
        if (string.IsNullOrEmpty(email))
            throw new InvalidOperationException("Aucun utilisateur connecté.");

        if (_data is not null && _email == email) return _data;

        _email = email;
        _data = new UserData();
        var path = PathFor(email);
        if (File.Exists(path))
        {
            try
            {
                _data = JsonSerializer.Deserialize<UserData>(await File.ReadAllTextAsync(path)) ?? new UserData();
            }
            catch
            {
                _data = new UserData();
            }
        }
        return _data;
    }

    public async Task SaveAsync()
    {
        if (_data is null || _email is null) return;
        await File.WriteAllTextAsync(PathFor(_email), JsonSerializer.Serialize(_data));
    }

    public async Task<(bool Ok, string? Error)> AddGoalAsync(LifeGoal goal)
    {
        var d = await GetAsync();
        if (d.Goals.Count(g => g.Status == GoalStatus.Active) >= MaxActiveGoals)
            return (false, "Tu as déjà 3 objectifs actifs. Termine ou archive-en un avant d'en ajouter.");

        d.Goals.Add(goal);
        await SaveAsync();
        return (true, null);
    }

    public async Task<(bool Ok, string? Error)> AddActionAsync(DailyAction action)
    {
        var d = await GetAsync();

        if (!d.Goals.Any(g => g.Id == action.GoalId && g.Status == GoalStatus.Active))
            return (false, "Une action doit être liée à un objectif actif.");

        if (d.Actions.Count(a => a.ScheduledDate.Date == action.ScheduledDate.Date) >= MaxActionsPerDay)
            return (false, "Tu as déjà 5 actions prévues ce jour-là.");

        action.ScheduledDate = action.ScheduledDate.Date;
        d.Actions.Add(action);
        await SaveAsync();
        return (true, null);
    }

    // Recrée chaque jour les actions « quotidiennes » des objectifs actifs (5 par jour maximum)
    public async Task EnsureTodayActionsAsync()
    {
        var d = await GetAsync();
        var today = DateTime.Today;
        var activeIds = d.Goals.Where(g => g.Status == GoalStatus.Active).Select(g => g.Id).ToHashSet();

        var templates = d.Actions
            .Where(a => a.Frequency == "Quotidienne" && activeIds.Contains(a.GoalId))
            .GroupBy(a => a.SeriesId)
            .Select(g => g.OrderByDescending(a => a.ScheduledDate).First())
            .ToList();

        var changed = false;
        foreach (var t in templates)
        {
            if (d.Actions.Any(a => a.SeriesId == t.SeriesId && a.ScheduledDate.Date == today)) continue;
            if (d.Actions.Count(a => a.ScheduledDate.Date == today) >= MaxActionsPerDay) break;

            d.Actions.Add(new DailyAction
            {
                SeriesId = t.SeriesId,
                GoalId = t.GoalId,
                Title = t.Title,
                Description = t.Description,
                ScheduledDate = today,
                EstimatedMinutes = t.EstimatedMinutes,
                Frequency = t.Frequency
            });
            changed = true;
        }

        if (changed) await SaveAsync();
    }

    // Estimation simple : actions réalisées / actions prévues, plafonnée à 95 %.
    // Seul le bouton « Atteint » (décision de l'utilisateur) fait passer un objectif à 100 %.
    public static double EstimateProgress(UserData data, LifeGoal goal)
    {
        if (goal.Status == GoalStatus.Achieved) return 1;
        var actions = data.Actions.Where(a => a.GoalId == goal.Id).ToList();
        if (actions.Count == 0) return 0;
        return Math.Min(0.95, actions.Count(a => a.IsDone) / (double)actions.Count);
    }

    private static string PathFor(string email)
    {
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(email)));
        return Path.Combine(FileSystem.AppDataDirectory, $"become_{hash}.json");
    }
}