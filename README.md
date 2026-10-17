# Become – application mobile .NET MAUI

Suivi d'objectifs avec jalons et mesure de la régularité.
**CRÉER → DÉCOUPER → AGIR → MESURER → PROGRESSER**

## Écrans (9)

| # | Écran | Fichiers |
|---|-------|----------|
| 1 | Connexion | `Views/LoginPage` · `ViewModels/LoginViewModel` |
| 2 | Inscription | `Views/RegisterPage` · `ViewModels/RegisterViewModel` |
| – | Mot de passe oublié | `Views/ForgotPasswordPage` · `ViewModels/ForgotPasswordViewModel` |
| 3 | Accueil | `Views/HomePage` · `ViewModels/HomeViewModel` |
| 4 | Mes objectifs | `Views/GoalsPage` · `ViewModels/GoalsViewModel` |
| 5 | Création d'un objectif | `Views/CreateGoalPage` · `ViewModels/CreateGoalViewModel` |
| 6 | Détail d'un objectif | `Views/GoalDetailPage` · `ViewModels/GoalDetailViewModel` |
| 7 | Suivi quotidien | `Views/TrackingPage` · `ViewModels/TrackingViewModel` |
| 8 | Bilan de progression | `Views/ReportPage` · `ViewModels/ReportViewModel` |

## Lancer le projet

1. Prérequis : Visual Studio 2022 17.8+ (charge de travail « Développement .NET Multiplateforme »)
   ou `dotnet workload install maui`, avec le SDK **.NET 8**.
2. Ouvre `Become.csproj`, choisis un émulateur Android (ou un appareil), puis **Exécuter**.
3. Les polices Inter (`Resources/Fonts`) et le logo (`Resources/Images/become_logo.png`)
   sont déjà dans le projet : remplace-les par tes exports si besoin
   (le logo doit rester en PNG, nom en minuscules).

## Tester tous les états

- **Connexion** : champs vides → erreurs ; mot de passe < 8 caractères → identifiants incorrects ;
  mode avion → erreur réseau. N'importe quel e-mail valide + 8 caractères ou plus fonctionne.
  Les objectifs de démonstration se chargent à la première connexion.
- **Inscription** : `test@become.app` → e-mail déjà utilisé ; CGU non cochées → erreur.
- **Accueil / Objectifs / Bilan** : vide (aucun objectif), chargement (squelettes), erreur.
- **Suivi** : valider / annuler une séance, feedback à trois niveaux (simple, série, jalon).
- **Création** : titre vide, date passée, aucun jour sélectionné, confirmation avant d'annuler.

## Choix d'écoconception

- Offline-first : tout est stocké dans un fichier JSON local, aucune requête pour l'usage quotidien.
- Aucune image décorative, aucune animation, un seul logo, icônes en glyphes.
- Composants réutilisés (`FormField`, `ProgressRing`, `LinearProgress`, `BottomNavBar`).
- Validation locale instantanée, calculs de progression faits sur l'appareil.

## À brancher pour la suite

- `Services/AuthService.cs` : remplace `FakeAuthService` par ton API.
- Rappels : `ReminderEnabled` / `ReminderTime` sont enregistrés ; l'envoi de notifications
  locales reste à ajouter (plugin de notifications locales).
- Icônes : `Helpers/Icons.cs` (glyphes) → remplace par tes SVG Figma.
