using AhAnalyticsPriceUpdater.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AhAnalyticsPriceUpdater.Services;

public class ScanDataDecrypter(ILogger logger, IConfiguration configuration)
{
    private const    string         ScanDataSourceFile = "ScanDataFunnel\\Auc-ScanData.lua";
    private readonly IConfiguration configuration      = configuration;
    private readonly ILogger        logger             = logger;

    public List<AuctionData> GetAllAuctions()
    {
        var auctionDataObjects = new List<AuctionData>();

        DoActionWithExceptionlogging(() =>
        {
            var directory = GetScanDataDirectory();
            var file      = File.ReadAllText(directory);

            var normalizedScanData = GetNormalizedScanData(file);

            foreach (var scanDataStringValue in normalizedScanData)
            {
                if (string.IsNullOrWhiteSpace(scanDataStringValue))
                    continue;

                var fields = scanDataStringValue.Split(',');

                if (!fields.Any() || fields.Length < 20)
                    continue;

                CreateAuctionDataObject(fields, auctionDataObjects);
            }
        });

        return GetAverageOfCheapestTenAuctionsOrLess(auctionDataObjects);
    }

    private static List<AuctionData> GetAverageOfCheapestTenAuctionsOrLess(List<AuctionData> auctionDataObjects)
    {
        var itemGroup = auctionDataObjects.GroupBy(ado => ado.ItemName);

        var cheapestAuctionsPerItem = new List<AuctionData>();

        foreach (var auctionDatas in itemGroup)
        {
            var withoutZeroPriceAsc = auctionDatas.Where(ad => ad.BuyoutInCopper != 0)
                                                  .OrderBy(ad => ad.BuyoutInCopper)
                                                  .ToList();

            var itemsToTake = 10;

            if(withoutZeroPriceAsc.Count == 0)
                continue;

            if (withoutZeroPriceAsc.Count < itemsToTake)
                itemsToTake = withoutZeroPriceAsc.Count;

            var selection = withoutZeroPriceAsc.Take(itemsToTake)
                                               .ToList();

            var totalPrice            = selection.Sum(s => s.BuyoutInCopper);
            var averageBuyoutInCopper = totalPrice / itemsToTake;

            var cheapestOne = selection.MinBy(ad => ad.BuyoutInCopper);

            if (cheapestOne == null)
                continue;

            cheapestOne.BuyoutInCopper = averageBuyoutInCopper;
            cheapestAuctionsPerItem.Add(cheapestOne);
        }

        return cheapestAuctionsPerItem;
    }

    private static List<string> GetNormalizedScanData(string file)
    {
        var resultStrings   = new List<string>();
        var relevantContent = file.Split("[\"ropes\"] = {")[1].Trim();

        relevantContent = RemoveEndingClump(relevantContent);

        StripBatches(relevantContent, resultStrings);

        return resultStrings;
    }

    private static string RemoveEndingClump(string relevantContent)
    {
        while (relevantContent.EndsWith('}') || relevantContent.EndsWith(','))
            relevantContent = relevantContent[..^1].Trim();

        return relevantContent;
    }

    private static void StripBatches(string relevantContent, List<string> resultStrings)
    {
        var batches = relevantContent.Split("\"return {{");

        foreach (var batch in batches)
        {
            var resultSet = batch.Split("},{");
            resultStrings.AddRange(resultSet);
        }
    }

    private static void CreateAuctionDataObject(string[] fields, List<AuctionData> auctionDataObjects)
    {
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
            logger.LogError(e.StackTrace);
        }
    }
}