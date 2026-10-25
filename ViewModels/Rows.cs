using System.Windows.Input;
using Become.Domain;

namespace Become.ViewModels;

public class SelectableChip : BecomeViewModelBase
{
    private bool _isSelected;

    public string Label { get; }
    public bool IsSelected { get => _isSelected; set => Set(ref _isSelected, value); }
    public ICommand ToggleCommand { get; }

    public SelectableChip(string label)
    {
        Label = label;
        ToggleCommand = new Command(() => IsSelected = !IsSelected);
    }
}

public class TodayActionRow : BecomeViewModelBase
{
    private bool _isDone;

    public Guid Id { get; }
    public string Title { get; }
    public string Subtitle { get; }
    public bool IsDone { get => _isDone; set => Set(ref _isDone, value); }
    public ICommand ToggleCommand { get; }
    public ICommand EditCommand { get; }

    public TodayActionRow(DailyAction action, string goalTitle,
        Func<TodayActionRow, Task> toggle, Func<TodayActionRow, Task> edit)
    {
        Id = action.Id;
        Title = action.Title;
        Subtitle = action.EstimatedMinutes is int m ? $"{goalTitle} · {m} min" : goalTitle;
        _isDone = action.IsDone;
        ToggleCommand = new Command(async () => await toggle(this));
        EditCommand = new Command(async () => await edit(this));
    }
}

public class GoalCardRow
{
    public Guid Id { get; }
    public string Title { get; }
    public string Description { get; }
    public string Domain { get; }
    public string ExpectedResult { get; }
    public string DueText { get; }
    public bool HasDue { get; }
    public double Progress { get; }
    public string ProgressText { get; }

    public ICommand EditCommand { get; }
    public ICommand AchieveCommand { get; }
    public ICommand ArchiveCommand { get; }

    public GoalCardRow(LifeGoal goal, double progress,
        Func<GoalCardRow, Task>? edit = null,
        Func<GoalCardRow, Task>? achieve = null,
        Func<GoalCardRow, Task>? archive = null)
    {
        Id = goal.Id;
        Title = goal.Title;
        Description = goal.Description;
        Domain = goal.Domain;
        ExpectedResult = $"Résultat attendu : {goal.ExpectedResult}";
        HasDue = goal.DueDate is not null;
        DueText = goal.DueDate is DateTime d ? $"Échéance : {d:dd/MM/yyyy}" : "";
        Progress = progress;
        ProgressText = $"Progression estimée : {(int)Math.Round(progress * 100)} %";

        EditCommand = new Command(async () => { if (edit is not null) await edit(this); });
        AchieveCommand = new Command(async () => { if (achieve is not null) await achieve(this); });
        ArchiveCommand = new Command(async () => { if (archive is not null) await archive(this); });
    }
}