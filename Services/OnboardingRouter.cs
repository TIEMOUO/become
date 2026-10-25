using Become.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Become.Services;

public static class OnboardingRouter
{
    public static async Task GoNextAsync()
    {
        var store = IPlatformApplication.Current!.Services.GetRequiredService<IBecomeStore>();
        var data = await store.GetAsync();

        string route;
        if (data.Vision is null)
            route = nameof(Views.VisionQuestionnairePage);
        else if (!data.Vision.IsValidated)
            route = nameof(Views.VisionResultPage);
        else if (data.Priorities is null)
            route = nameof(Views.PrioritiesPage);
        else if (data.Goals.Count == 0)
            route = nameof(Views.GoalSuggestionsPage);
        else
            route = "//MainTabs";

        await Shell.Current.GoToAsync(route);
    }
}