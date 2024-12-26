namespace AhAnalyticsPriceUpdater.Interfaces;

public interface IProgressbarFeeder
{
    public delegate void ScanningProgressedEventHandler(object sender, double progress);

    public event ScanningProgressedEventHandler? ScanningProgressed;
}