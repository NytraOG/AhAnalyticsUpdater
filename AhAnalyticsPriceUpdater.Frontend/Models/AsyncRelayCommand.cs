using System.Windows.Input;

namespace AhAnalyticsPriceUpdater.Frontend.Models;

public class AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute = null!) : ICommand
{
    private readonly Func<Task> execute    = execute ?? throw new ArgumentNullException(nameof(execute));
    private          bool       isExecuting;

    public bool CanExecute(object? parameter) => !isExecuting && (canExecute?.Invoke() ?? true);

    public async void Execute(object? parameter)
    {
        isExecuting = true;
        RaiseCanExecuteChanged();

        try
        {
            await execute();
        }
        finally
        {
            isExecuting = false;
            RaiseCanExecuteChanged();
        }
    }

    public event EventHandler? CanExecuteChanged;

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}