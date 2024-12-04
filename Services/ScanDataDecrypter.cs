using AhAnalyticsPriceUpdater.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AhAnalyticsPriceUpdater.Services;

public class ScanDataDecrypter(ILogger logger, IConfiguration configuration)
{
    private const    string         ScanDataSourceFile = "ScanDataFunnel\\Auc-ScanData.lua";
    private readonly IConfiguration configuration      = configuration;
    private readonly ILogger        logger             = logger;

    public IEnumerable<AuctionData> GetAllAuctions()
    {
        var auctionDataObjects = new List<AuctionData>();

        DoActionWithExceptionlogging(() =>
        {
            var directory = GetScanDataDirectory();
            var file      = File.ReadAllText(directory);

            var relevanterContent = file.Split("[\"ropes\"] = {")[1].Trim();

            while (relevanterContent.EndsWith('}') || relevanterContent.EndsWith(','))
                relevanterContent = relevanterContent[..^1].Trim();

            var resultStrings = new List<string>();
            var batches       = relevanterContent.Split("\"return {{");

            foreach (var batch in batches)
            {
                var resultSet = batch.Split("},{");
                resultStrings.AddRange(resultSet);
            }

            foreach (var result in resultStrings)
            {
                if (string.IsNullOrWhiteSpace(result))
                    continue;

                var fields = result.Split(',');

                if (!fields.Any() || fields.Length < 20)
                    continue;

                var obj = new AuctionData
                {
                    ItemName       = fields[8].Replace("\\\"", string.Empty),
                    StackSize      = int.Parse(fields[10]),
                    MinLvl         = int.Parse(fields[13]),
                    BuyoutInCopper = int.Parse(fields[16]),
                    Seller         = fields[19].Replace("\\\"", string.Empty)
                };

                auctionDataObjects.Add(obj);
            }
        });

        return auctionDataObjects;
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