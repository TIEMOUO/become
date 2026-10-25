using System.ComponentModel;
using System.Net.Mail;
using System.Runtime.CompilerServices;

namespace Become.ViewModels;

public abstract class BecomeViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(name);
        return true;
    }

    // Alias utilisé par LoginViewModel et RegisterViewModel
    protected bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null) =>
        SetProperty(ref field, value, name);

    protected static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        try { return new MailAddress(email.Trim()).Address == email.Trim(); }
        catch { return false; }
    }
}