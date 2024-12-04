using AhAnalyticsPriceUpdater.Models;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;

namespace AhAnalyticsPriceUpdater.Services;

public class SpreadsheetService
{
    private const    string            RelativeFileDirectory = "Spreadsheets\\AhAnalytics.xlsx";
    private readonly ILogger           logger;
    private readonly ScanDataDecrypter scanDataDecrypter;
    private          ExcelWorksheet    matsSheet;
    private          ExcelWorkbook     workbook;

    public SpreadsheetService(ScanDataDecrypter scanDataDecrypter,
                              ILogger           logger)
    {
        Initialize();

        this.scanDataDecrypter = scanDataDecrypter;
        this.logger            = logger;
    }

    private void Initialize() => ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

    public void UpdateSpreadsheet() => DoActionWithExceptionlogging(() =>
    {
        var auctions = scanDataDecrypter.GetAllAuctions();

        UpdateMaterialPrices(auctions);
        UpdateSellingMarketprices(auctions);
    });

    private void UpdateMaterialPrices(List<AuctionData> auctions)
    {
        var cellMaterials = GetMaterialsFromCells();

        UpdateTemporaryMaterialPrices(cellMaterials, auctions);
        SaveMaterialPricesToSheet(cellMaterials);
    }

    private void UpdateSellingMarketprices(List<AuctionData> auctions)
    {
        var       directory   = GetSpreadsheetDirectory();
        using var exclPackage = new ExcelPackage(new FileInfo(directory));
        var       book        = exclPackage.Workbook;
        var       mainSheet   = book.Worksheets[0];

        var cellMarketitems = GetMarketItemsFromCells(mainSheet);

        UpdateTemporaryMarketItemPrices(auctions, cellMarketitems);
        SaveMarketItemPricesToSheet(cellMarketitems, mainSheet);

        var fileInfo = new FileInfo(directory);
        exclPackage.SaveAs(fileInfo);
    }

    private void SaveMarketItemPricesToSheet(List<CellMarketItem> cellMarketitems, ExcelWorksheet mainSheet)
    {
        foreach (var cellMarketItem in cellMarketitems)
        {
            if (cellMarketItem.Price == 0)
                continue;

            var cell = mainSheet.Cells.FirstOrDefault(c => c.Address == cellMarketItem.CellAddressToUpdate);

            if (cell is null)
                continue;

            cell.Value = cellMarketItem.Price;
        }
    }

    private void UpdateTemporaryMarketItemPrices(List<AuctionData> auctions, List<CellMarketItem> cellMarketitems)
    {
        foreach (var cellMarketItem in cellMarketitems)
        {
            var fittingAuction = auctions.FirstOrDefault(a => a.ItemName == cellMarketItem.MarketItem);

            if (fittingAuction is null)
            {
                logger.LogInformation($"No fitting auction found for Item '{cellMarketItem.MarketItem}' from Cell {cellMarketItem.CellAddressToUpdate}");
                continue;
            }

            cellMarketItem.Price = fittingAuction.BuyoutInSilver;
        }
    }

    private static List<CellMarketItem> GetMarketItemsFromCells(ExcelWorksheet mainSheet)
    {
        var cellMarketitems = new List<CellMarketItem>();

        var cells = mainSheet.Cells.ToArray();

        for (var i = 2; i < cells.Length; i++)
        {
            var rowNumber     = i;
            var relevantCells = cells.Where(c => c.Address.Contains($"{rowNumber}")).ToArray();

            if (relevantCells.Length < 2 || relevantCells[0]?.Value is null)
                continue;

            var marketItem = relevantCells[0].Value.ToString();
            var price      = Convert.ToDecimal(relevantCells[1].Value);

            var cellMaterial = new CellMarketItem
            {
                MarketItem          = marketItem,
                Price               = price,
                CellAddressToUpdate = relevantCells[1].Address
            };

            cellMarketitems.Add(cellMaterial);
        }

        return cellMarketitems;
    }

    private void SaveMaterialPricesToSheet(List<CellMaterial> cellMaterials)
    {
        var       directory   = GetSpreadsheetDirectory();
        using var exclPackage = new ExcelPackage(new FileInfo(directory));
        workbook  = exclPackage.Workbook;
        matsSheet = workbook.Worksheets[2];

        foreach (var cellMaterial in cellMaterials)
        {
            if (cellMaterial.Material == "Empty Vial" || cellMaterial.Material == "Leaded Vial" || cellMaterial.Material == "Gilded Vial")
                continue;

            if (cellMaterial.Price == 0)
                continue;

            var cell = matsSheet.Cells.FirstOrDefault(c => c.Address == cellMaterial.CellAddressToUpdate);

            if (cell is null)
                continue;

            cell.Value = cellMaterial.Price;
        }

        var fileInfo = new FileInfo(directory);
        exclPackage.SaveAs(fileInfo);
    }

    private void UpdateTemporaryMaterialPrices(List<CellMaterial> cellMaterials, List<AuctionData> auctions)
    {
        foreach (var cellMaterial in cellMaterials)
        {
            var fittingAuction = auctions.FirstOrDefault(a => a.ItemName == cellMaterial.Material);

            if (fittingAuction is null)
            {
                logger.LogInformation($"No fitting auction found for Item '{cellMaterial.Material}' from Cell {cellMaterial.CellAddressToUpdate}");
                continue;
            }

            cellMaterial.Price = fittingAuction.BuyoutInSilver;
        }
    }

    private List<CellMaterial> GetMaterialsFromCells()
    {
        var directory = GetSpreadsheetDirectory();

        using var exclPackage = new ExcelPackage(new FileInfo(directory));
        workbook  = exclPackage.Workbook;
        matsSheet = workbook.Worksheets[2];

        var cells     = matsSheet.Cells.ToArray();
        var resultSet = new List<CellMaterial>();

        for (var i = 2; i < cells.Length; i++)
        {
            var rowNumber     = i;
            var relevantCells = cells.Where(c => c.Address.Contains($"{rowNumber}")).ToArray();

            if (relevantCells.Length < 2)
                continue;

            var material = relevantCells[0].Value.ToString();
            var price    = Convert.ToDecimal(relevantCells[1].Value);

            var cellMaterial = new CellMaterial
            {
                Material            = material,
                Price               = price,
                CellAddressToUpdate = relevantCells[1].Address
            };

            resultSet.Add(cellMaterial);
        }

        return resultSet;
    }

    private string GetSpreadsheetDirectory()
    {
        var baseDirectory     = Directory.GetCurrentDirectory();
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
            logger.LogError(e.StackTrace);
        }
    }
}