using System.Windows.Input;

namespace AhAnalyticsPriceUpdater.Frontend.Models;

public class AsyncRelayCommand : ICommand
{
    private readonly Func<bool> canExecute;
    private readonly Func<Task> execute;
    private          bool       isExecuting;

    public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute = null!)
    {
        this.execute    = execute ?? throw new ArgumentNullException(nameof(execute));
        this.canExecute = canExecute;
    }

    public AsyncRelayCommand(Action execute, Func<bool> canExecute = null!)
    {
        this.canExecute = canExecute;

        this.execute = () =>
        {
            execute();
            return Task.CompletedTask;
        };
    }

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