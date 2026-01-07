using AhAnalyticsPriceUpdater.Interfaces;
using AhAnalyticsPriceUpdater.Models;
using Microsoft.Extensions.Logging;

namespace AhAnalyticsPriceUpdater.Services;

public class ScanDataDecrypter(ILogger<ScanDataDecrypter> logger) : IProgressbarFeeder
{
    private const string ScanDataSourceFile = "ScanDataFunnel\\Auc-ScanData.lua";

    public event IProgressbarFeeder.ScanningProgressedEventHandler? ScanningProgressed;
    public event IProgressbarFeeder.ScanningCompletedEventHandler?  ScanningCompleted;

    public List<AuctionData> GetAllAuctions(string? scanDataDirectory)
    {
        var auctionDataObjects = new List<AuctionData>();

        DoActionWithExceptionlogging(() =>
        {
            //var file = File.ReadAllText(scanDataDirectory!);
            var file                    = File.ReadAllText(ScanDataSourceFile);
            var normalizedScanData      = GetNormalizedScanData(file);
            var progressPerScanDataLine = 1 / (double)normalizedScanData.Count;

            foreach (var scanDataStringValue in normalizedScanData)
            {
                if (string.IsNullOrWhiteSpace(scanDataStringValue))
                {
                    ScanningProgressed?.Invoke(this, progressPerScanDataLine);
                    continue;
                }

                var fields = scanDataStringValue.Split(',');

                if (!fields.Any() || fields.Length < 20)
                    continue;

                CreateAuctionDataObject(fields, auctionDataObjects);
                ScanningProgressed?.Invoke(this, progressPerScanDataLine);
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

            if (withoutZeroPriceAsc.Count == 0)
                continue;

            if (withoutZeroPriceAsc.Count < itemsToTake)
                itemsToTake = withoutZeroPriceAsc.Count;

            var selection = withoutZeroPriceAsc.Take(itemsToTake)
                .ToList();

            var totalPrice = selection.Sum(s => s.BuyoutInCopper);
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
        var resultStrings      = new List<string>();
        var contentSchlachtgut = file.Split("[\"ropes\"] = {");
        var relevantContent    = contentSchlachtgut[3].Trim();

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
        AuctionData obj;

        try
        {
            var stackSize      = int.TryParse(fields[10], out var stackSizeOut) ? stackSizeOut : int.Parse(fields[12]);
            var minLvl         = int.TryParse(fields[13], out var minLvlOut) ? minLvlOut : int.Parse(fields[15]);
            var buyoutInCopper = int.TryParse(fields[16], out var buyoutInCopperOut) ? buyoutInCopperOut : int.Parse(fields[18]);

            obj = new AuctionData
            {
                ItemName       = fields.Length > 28 ? fields[8].Replace("\\\"", string.Empty) + fields[9].Replace("\\\"", string.Empty) : fields[8].Replace("\\\"", string.Empty),
                StackSize      = stackSize,
                MinLvl         = minLvl,
                BuyoutInCopper = buyoutInCopper,
                Seller         = fields[19].Replace("\\\"", string.Empty)
            };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        auctionDataObjects.Add(obj);
    }

    private string GetScanDataDirectory()
    {
        var baseDirectory = Directory.GetCurrentDirectory();
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