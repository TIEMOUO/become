using Become.Models;

namespace Become.ViewModels;

/// <summary>
/// Base des écrans qui valident des séances : message de retour discret, qui disparaît seul.
/// Trois niveaux de feedback : séance simple, série qui progresse, jalon / objectif atteint.
/// </summary>
public abstract class FeedbackViewModel : BaseViewModel
{
    private string _feedback = "";
    private CancellationTokenSource? _cts;

    public string Feedback
    {
        get => _feedback;
        private set { if (SetProperty(ref _feedback, value)) OnPropertyChanged(nameof(HasFeedback)); }
    }
    public bool HasFeedback => !string.IsNullOrEmpty(Feedback);

    protected async void ShowFeedback(string message, int milliseconds = 4000)
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        Feedback = message;
        try
        {
            await Task.Delay(milliseconds, token);
            Feedback = "";
        }
        catch (TaskCanceledException)
        {
            // un nouveau message a pris la place
        }
    }

    protected void HideFeedback()
    {
        _cts?.Cancel();
        Feedback = "";
    }

    protected static string Describe(ToggleResult r, Goal goal)
    {
        if (!r.Validated) return "Validation annulée.";
        if (r.GoalAchieved) return $"Objectif atteint : {goal.Title}. Bravo !";
        if (r.ReachedMilestone is not null) return $"Jalon atteint : {r.ReachedMilestone.Title}.";
        if (r.StreakAfter > r.StreakBefore)
            return r.StreakAfter <= 1 ? "Série lancée. Beau début !" : $"Série : {r.StreakAfter} jours. Gardez l'élan !";
        return "Séance validée.";
    }
}
