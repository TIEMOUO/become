using Become.Domain;

namespace Become.Services;

public record ActionSuggestion(string Title, string Description, int Minutes);

public record GoalSuggestion(
    string Domain,
    string Title,
    string Description,
    string ExpectedResult,
    string VisionLink,
    List<ActionSuggestion> Actions);

public static class VisionWriter
{
    // Assemblage simple des 4 réponses : ce n'est pas de l'IA, l'utilisateur modifie ensuite le texte.
    public static string Compose(VisionProfile v)
    {
        static string Clean(string s) => s.Trim().TrimEnd('.', ' ', ';');

        return $"Je veux devenir {Clean(v.WhoIWantToBecome)}. " +
               $"Ce qui compte le plus pour moi : {Clean(v.ImportantAreas)}. " +
               $"Mes valeurs et mes aspirations : {Clean(v.ValuesAndAspirations)}. " +
               $"La vie que je veux construire : {Clean(v.LifeIWantToBuild)}.";
    }
}

public static class GoalEngine
{
    private sealed record Template(string Title, string Description, string Result, (string Title, string Desc)[] Actions);

    public static GoalSuggestion Suggest(string domain, int variant, VisionProfile vision, PrioritiesProfile priorities, int plannedGoals)
    {
        var templates = TemplatesFor(domain);
        var t = templates[Math.Abs(variant) % templates.Length];

        var goals = Math.Max(1, plannedGoals);
        var actionsPerGoal = goals >= 3 ? 1 : 2;
        var perGoalMinutes = Math.Max(5, priorities.MinutesPerDay / goals);
        var chosen = t.Actions.Take(actionsPerGoal).ToList();
        var each = Math.Max(5, (int)(Math.Round(perGoalMinutes / (double)chosen.Count / 5.0) * 5));

        var actions = chosen.Select(a => new ActionSuggestion(a.Title, a.Desc, each)).ToList();

        var description = t.Description;
        if (!string.IsNullOrWhiteSpace(priorities.Constraints))
            description += $" Contrainte notée : {Truncate(priorities.Constraints.Trim(), 80)}";

        var link = $"Cet objectif te rapproche de ta vision : « {Truncate(vision.WhoIWantToBecome.Trim(), 90)} »";

        return new GoalSuggestion(domain, t.Title, description, t.Result, link, actions);
    }

    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : s[..(max - 1)].TrimEnd() + "…";

    private static Template[] TemplatesFor(string domain) => domain switch
    {
        LifeDomains.Career => new[]
        {
            new Template("Progresser dans mes études et ma carrière",
                "Dégager un temps régulier pour développer les compétences qui comptent pour la personne que je veux devenir.",
                "Avoir acquis une compétence concrète, visible dans mon travail ou mes études, d'ici trois mois.",
                new[] { ("Travailler une compétence clé", "Une session ciblée, sans distraction."),
                        ("Noter ce que j'ai appris", "Deux ou trois lignes pour fixer l'essentiel.") }),
            new Template("Construire un projet qui prouve mes compétences",
                "Faire avancer un projet concret qui montre ce que je sais faire.",
                "Avoir une première version présentable de mon projet.",
                new[] { ("Avancer sur mon projet", "Terminer une petite étape."),
                        ("Définir la prochaine étape", "Écrire ce que je ferai demain.") })
        },
        LifeDomains.Finance => new[]
        {
            new Template("Prendre le contrôle de mes finances",
                "Comprendre où va mon argent et le suivre sans effort.",
                "Connaître mes dépenses du mois et suivre un budget simple.",
                new[] { ("Noter mes dépenses du jour", "Toutes, même les petites."),
                        ("Vérifier mon budget", "Comparer avec ce que j'avais prévu.") }),
            new Template("Mettre de côté régulièrement",
                "Créer l'habitude d'épargner, même de petits montants.",
                "Avoir constitué le premier montant d'épargne que je me suis fixé.",
                new[] { ("Mettre une petite somme de côté", "Même un petit montant compte."),
                        ("Repérer une dépense à réduire", "Une seule, la plus évidente.") })
        },
        LifeDomains.Health => new[]
        {
            new Template("Prendre soin de ma santé au quotidien",
                "Installer des gestes simples pour mon énergie et mon bien-être.",
                "Avoir tenu mes habitudes de santé la plupart des jours pendant un mois.",
                new[] { ("Bouger mon corps", "Marche, étirements ou sport léger."),
                        ("Faire une pause de récupération", "Quelques minutes pour respirer et relâcher la tension.") }),
            new Template("Retrouver un bon rythme de sommeil",
                "Me coucher et me lever à des heures plus régulières.",
                "Avoir un rythme de sommeil stable la plupart des nuits.",
                new[] { ("Préparer ma soirée", "Réduire les écrans avant de dormir."),
                        ("Noter mon heure de coucher", "Pour repérer mes habitudes.") })
        },
        LifeDomains.Relations => new[]
        {
            new Template("Entretenir mes relations importantes",
                "Rester présent(e) pour les personnes qui comptent.",
                "Avoir pris régulièrement des nouvelles des personnes importantes pour moi.",
                new[] { ("Prendre des nouvelles d'un proche", "Un message ou un appel."),
                        ("Remercier ou encourager quelqu'un", "Un geste simple.") }),
            new Template("Passer du temps de qualité avec ceux que j'aime",
                "Créer des moments partagés, sans distraction.",
                "Avoir vécu chaque semaine un moment de qualité planifié.",
                new[] { ("Planifier un moment ensemble", "Fixer une date et un lieu."),
                        ("Être pleinement présent(e)", "Téléphone de côté pendant un échange.") })
        },
        LifeDomains.Growth => new[]
        {
            new Template("Cultiver mes habitudes de croissance",
                "Apprendre et réfléchir un peu chaque jour.",
                "Avoir installé une habitude quotidienne d'apprentissage.",
                new[] { ("Lire ou écouter quelque chose d'utile", "Un livre, un article, un podcast."),
                        ("Écrire une pensée du jour", "Ce que j'ai compris ou ressenti.") }),
            new Template("Mieux me connaître",
                "Prendre du recul sur mes forces, mes limites et mes envies.",
                "Avoir identifié mes trois forces et un axe de progrès.",
                new[] { ("Faire le point sur ma journée", "Qu'est-ce qui a bien marché ?"),
                        ("Identifier une habitude à améliorer", "Une seule, bien précise.") })
        },
        LifeDomains.Spirituality => new[]
        {
            new Template("Nourrir ma vie spirituelle",
                "Garder un temps régulier pour ma relation avec ce qui me dépasse.",
                "Avoir un temps spirituel régulier presque chaque jour.",
                new[] { ("Temps de prière ou de méditation", "Un moment de calme."),
                        ("Lire un passage et le noter", "Garder une phrase à retenir.") }),
            new Template("Vivre mes valeurs spirituelles au quotidien",
                "Traduire ma foi ou mes convictions en gestes concrets.",
                "Avoir posé chaque semaine un acte concret en accord avec mes valeurs.",
                new[] { ("Choisir un acte à poser", "Petit et concret."),
                        ("Faire un bilan de fin de journée", "Ai-je vécu ce que je crois ?") })
        },
        _ => new[]
        {
            new Template($"Avancer sur : {domain}",
                "Consacrer un temps régulier à ce domaine qui compte pour moi en ce moment.",
                "Avoir franchi une étape concrète et mesurable dans ce domaine d'ici un mois.",
                new[] { ("Faire un pas concret", "Une action simple et précise dans ce domaine."),
                        ("Noter ce que j'ai accompli", "Une ligne suffit.") }),
            new Template($"Clarifier ce que je veux dans : {domain}",
                "Définir ce que « réussir » signifie pour moi dans ce domaine.",
                "Avoir une définition claire et écrite de mon objectif dans ce domaine.",
                new[] { ("Réfléchir à mon objectif", "Écrire ce que je veux atteindre."),
                        ("Lister un obstacle et une solution", "Un seul, le plus important.") })
        }
    };
}