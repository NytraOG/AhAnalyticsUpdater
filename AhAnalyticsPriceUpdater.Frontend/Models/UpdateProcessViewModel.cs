using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace AhAnalyticsPriceUpdater.Frontend.Models;

public class UpdateProcessViewModel : INotifyPropertyChanged
{
    public ICommand                           StartProcess { get; }
    public bool                               InProgress   { get; set; }
    public event PropertyChangedEventHandler? PropertyChanged;

    public UpdateProcessViewModel()
    {
        StartProcess = new RelayCommand(Start);
    }
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    public void Start() => InProgress = true;
}