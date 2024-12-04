using AhAnalyticsPriceUpdater.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AhAnalyticsPriceUpdater.Services;

public class ScanDataDecrypter(ILogger logger, IConfiguration configuration)
{
    private readonly ILogger        logger             = logger;
    private readonly IConfiguration configuration      = configuration;
    private const    string         ScanDataSourceFile = "ScanDataFunnel\\Auc-ScanData.lua";

    public IEnumerable<AuctionData> GetAllAuctions()
    {
        DoActionWithExceptionlogging(() =>
        {
            var directory = GetScanDataDirectory();
            var file      = File.ReadAllText(directory);



            // var lines = File.ReadAllLines(file)
            //                 .ToArray();
        });

        return Array.Empty<AuctionData>();
    }

    private string GetScanDataDirectory()
    {
        var baseDirectory     = Directory.GetCurrentDirectory();
        var scanDataDirectory = Path.Combine(baseDirectory, ScanDataSourceFile);

        if (scanDataDirectory is null)
            throw new Exception("No ScanData Directory configured!");

        return scanDataDirectory;
    }

    private void DoActionWithExceptionlogging(Action action)
    {
        try
        {
            action();
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
        }
    }
}