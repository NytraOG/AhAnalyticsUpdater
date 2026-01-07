namespace AhAnalyticsPriceUpdater.Interfaces;

public interface IProgressbarFeeder
{
    public delegate void ScanningProgressedEventHandler(object sender, double progress);
    public delegate void ScanningCompletedEventHandler();

    public event ScanningProgressedEventHandler? ScanningProgressed;
    public event ScanningCompletedEventHandler? ScanningCompleted;
}