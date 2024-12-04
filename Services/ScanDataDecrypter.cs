using Microsoft.Extensions.Logging;

namespace AhAnalyticsPriceUpdater.Services;

public class ScanDataDecrypter(ILogger logger)
{
    private readonly ILogger logger = logger;

    private void DoActionWithExceptionlogging(Action action)
    {
        try
        {
            action();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Knall weg :C");
        }
    }
}