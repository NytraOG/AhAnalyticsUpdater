using System.Diagnostics;
using AhAnalyticsPriceUpdater.Interfaces;
using AhAnalyticsPriceUpdater.Models;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;

namespace AhAnalyticsPriceUpdater.Services;

public class SpreadsheetService : IProgressbarFeeder
{
    private const string RelativeFileDirectory = "Spreadsheets\\AhAnalytics.xlsx";
    private readonly ILogger<SpreadsheetService> logger;
    private readonly ScanDataDecrypter scanDataDecrypter;
    private ExcelWorksheet? matsSheet;
    private readonly double scanningSegments = 3;
    private double totalScanningProgress;
    private ExcelWorkbook? workbook;

    public SpreadsheetService(ScanDataDecrypter scanDataDecrypter, ILogger<SpreadsheetService> logger)
    {
        Initialize();

        scanDataDecrypter.ScanningProgressed += ScanDataDecrypterOnScanningProgressed;

        this.scanDataDecrypter = scanDataDecrypter;
        this.logger = logger;
    }

    public event IProgressbarFeeder.ScanningProgressedEventHandler? ScanningProgressed;

    private void ScanDataDecrypterOnScanningProgressed(object sender, double progress)
    {
        totalScanningProgress += progress / scanningSegments;
        ScanningProgressed?.Invoke(this, totalScanningProgress);
    }

    private void Initialize()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public void UpdateSpreadsheet(string? installationRootWorldOfWarcraft)
    {
        DoActionWithExceptionlogging(() =>
        {
            var auctions = scanDataDecrypter.GetAllAuctions(installationRootWorldOfWarcraft);

            UpdateMaterialPrices(auctions);
            UpdateSellingMarketprices(auctions);
        });
    }

    public void OpenSpreadsheet()
    {
        Process.Start(new ProcessStartInfo
        {
            UseShellExecute = true,
            FileName = GetSpreadsheetDirectory()
        });
    }

    private void UpdateMaterialPrices(List<AuctionData> auctions)
    {
        var cellMaterials = GetMaterialsFromCells();

        UpdateTemporaryMaterialPrices(cellMaterials, auctions);
        SaveMaterialPricesToSheet(cellMaterials);
    }

    private void UpdateSellingMarketprices(List<AuctionData> auctions)
    {
        var directory = GetSpreadsheetDirectory();
        using var exclPackage = new ExcelPackage(new FileInfo(directory));
        var book = exclPackage.Workbook;
        var mainSheet = book.Worksheets[0];

        var cellMarketitems = GetMarketItemsFromCells(mainSheet);

        UpdateTemporaryMarketItemPrices(auctions, cellMarketitems);
        SaveMarketItemPricesToSheet(cellMarketitems, mainSheet);

        var fileInfo = new FileInfo(directory);
        exclPackage.SaveAs(fileInfo);
    }

    private void SaveMarketItemPricesToSheet(List<CellMarketItem> cellMarketitems, ExcelWorksheet mainSheet)
    {
        var scanningProgressPerMarketItem = 1 / (cellMarketitems.Count * scanningSegments * 3);
        foreach (var cellMarketItem in cellMarketitems)
        {
            if (cellMarketItem.Price == 0)
            {
                ScanningProgressed?.Invoke(this, scanningProgressPerMarketItem);
                continue;
            }

            var cell = mainSheet.Cells.FirstOrDefault(c => c.Address == cellMarketItem.CellAddressToUpdate);

            if (cell is null)
            {
                ScanningProgressed?.Invoke(this, scanningProgressPerMarketItem);
                continue;
            }

            cell.Value = cellMarketItem.Price;
            ScanningProgressed?.Invoke(this, scanningProgressPerMarketItem);
        }
    }

    private void UpdateTemporaryMarketItemPrices(List<AuctionData> auctions, List<CellMarketItem> cellMarketitems)
    {
        var scanningProgressPerMarketItem = 1 / (cellMarketitems.Count * scanningSegments * 3);
        
        foreach (var cellMarketItem in cellMarketitems)
        {
            var fittingAuction = auctions.FirstOrDefault(a => a.ItemName == cellMarketItem.MarketItem);

            if (fittingAuction is null)
            {
                logger.LogInformation(
                    $"No fitting auction found for Item '{cellMarketItem.MarketItem}' from Cell {cellMarketItem.CellAddressToUpdate}");

                ScanningProgressed?.Invoke(this, scanningProgressPerMarketItem);
                continue;
            }

            cellMarketItem.Price = fittingAuction.BuyoutInSilver;
            ScanningProgressed?.Invoke(this, scanningProgressPerMarketItem);
        }
    }

    private List<CellMarketItem> GetMarketItemsFromCells(ExcelWorksheet mainSheet)
    {
        var cellMarketitems = new List<CellMarketItem>();

        var cells = mainSheet.Cells.ToArray();
        var scanningProgressPerCell = 1 / (cells.Length * scanningSegments * 3);

        for (var i = 2; i < cells.Length; i++)
        {
            var rowNumber = i;
            var relevantCells = cells.Where(c => c.Address.Contains($"{rowNumber}")).ToArray();

            if (relevantCells.Length < 2 || relevantCells[0]?.Value is null)
            {
                ScanningProgressed?.Invoke(this, scanningProgressPerCell);
                continue;
            }

            var marketItem = relevantCells[0].Value.ToString();
            var price = Convert.ToDecimal(relevantCells[1].Value);

            var cellMaterial = new CellMarketItem
            {
                MarketItem = marketItem,
                Price = price,
                CellAddressToUpdate = relevantCells[1].Address
            };

            cellMarketitems.Add(cellMaterial);
            ScanningProgressed?.Invoke(this, scanningProgressPerCell);
        }

        return cellMarketitems;
    }

    private void SaveMaterialPricesToSheet(List<CellMaterial> cellMaterials)
    {
        var directory = GetSpreadsheetDirectory();
        using var exclPackage = new ExcelPackage(new FileInfo(directory));
        workbook = exclPackage.Workbook;
        matsSheet = workbook.Worksheets[2];

        var scanningProgressPerMaterial = 1 / cellMaterials.Count / 3;

        foreach (var cellMaterial in cellMaterials)
        {
            if (cellMaterial.Material == "Empty Vial" || cellMaterial.Material == "Leaded Vial" ||
                cellMaterial.Material == "Gilded Vial")
                continue;

            if (cellMaterial.Price == 0)
            {
                ScanningProgressed?.Invoke(this, scanningProgressPerMaterial);
                continue;
            }

            var cell = matsSheet.Cells.FirstOrDefault(c => c.Address == cellMaterial.CellAddressToUpdate);

            if (cell is null)
                continue;

            cell.Value = cellMaterial.Price;
            ScanningProgressed?.Invoke(this, scanningProgressPerMaterial);
        }

        var fileInfo = new FileInfo(directory);
        exclPackage.SaveAs(fileInfo);
    }

    private void UpdateTemporaryMaterialPrices(List<CellMaterial> cellMaterials, List<AuctionData> auctions)
    {
        var cellMaterialsAMount = cellMaterials.Count;
        var scanningProgressPerMaterial = 1 / (cellMaterialsAMount * scanningSegments * 3);
        
        foreach (var cellMaterial in cellMaterials)
        {
            var fittingAuction = auctions.FirstOrDefault(a => a.ItemName == cellMaterial.Material);

            if (fittingAuction is null)
            {
                logger.LogInformation($"No fitting auction found for Item '{cellMaterial.Material}' from Cell {cellMaterial.CellAddressToUpdate}");
                ScanningProgressed?.Invoke(this, scanningProgressPerMaterial);

                continue;
            }

            cellMaterial.Price = fittingAuction.BuyoutInSilver;
            ScanningProgressed?.Invoke(this, scanningProgressPerMaterial);
        }
    }

    private List<CellMaterial> GetMaterialsFromCells()
    {
        var directory = GetSpreadsheetDirectory();

        using var exclPackage = new ExcelPackage(new FileInfo(directory));
        workbook = exclPackage.Workbook;
        matsSheet = workbook.Worksheets[2];

        var cells = matsSheet.Cells.ToArray();
        var resultSet = new List<CellMaterial>();
        var cellsAmount = cells.Length;
        var scanningProgressPerCell = 1 / (cellsAmount * scanningSegments*3); 

        for (var i = 2; i < cellsAmount; i++)
        {
            var rowNumber = i;
            var relevantCells = cells.Where(c => c.Address.Contains($"{rowNumber}")).ToArray();

            if (relevantCells.Length < 2)
            {
                ScanningProgressed?.Invoke(this, scanningProgressPerCell);
                continue;
            }

            var material = relevantCells[0].Value.ToString();
            var price = Convert.ToDecimal(relevantCells[1].Value);

            var cellMaterial = new CellMaterial
            {
                Material = material,
                Price = price,
                CellAddressToUpdate = relevantCells[1].Address
            };

            resultSet.Add(cellMaterial);
            ScanningProgressed?.Invoke(this, scanningProgressPerCell);
        }

        return resultSet;
    }

    private string GetSpreadsheetDirectory()
    {
        var baseDirectory = Directory.GetCurrentDirectory();
        var scanDataDirectory = Path.Combine(baseDirectory, RelativeFileDirectory);

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
            logger.LogError(e.StackTrace);
            throw;
        }
    }
}