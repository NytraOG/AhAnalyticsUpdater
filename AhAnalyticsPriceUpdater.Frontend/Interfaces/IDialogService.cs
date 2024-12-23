namespace AhAnalyticsPriceUpdater.Frontend.Interfaces;

public interface IDialogService
{
    Task<string> SelectDirectory(string description = "Select a folder");
}