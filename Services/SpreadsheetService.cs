using AhAnalyticsPriceUpdater.Models;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;

namespace AhAnalyticsPriceUpdater.Services;

public class SpreadsheetService
{
    private const    string             Filename = "Spreadsheets\\AhAnalytics.xlsx";
    private readonly ILogger            logger;
    private readonly ScanDataDecrypter  scanDataDecrypter;
    private          List<CellMaterial> cellMaterials;

    public SpreadsheetService(ScanDataDecrypter scanDataDecrypter,
                              ILogger           logger)
    {
        Initialize();

        this.scanDataDecrypter = scanDataDecrypter;
        this.logger            = logger;
    }

    private void Initialize() => ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

    public void UpdateSpreadsheet()
    {
        GetMatsDictionary();

        var auctions = scanDataDecrypter.GetAllAuctions();
    }

    private void GetMatsDictionary()
    {
        var directory = GetSpreadsheetDirectory();

        using var exclPackage = new ExcelPackage(new FileInfo(directory));
        var       matsSheet   = exclPackage.Workbook.Worksheets[2];
        var       cells       = matsSheet.Cells.ToArray();

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

        cellMaterials = resultSet;
    }

    public void UpdateSellingMarketprices() { }

    private string GetSpreadsheetDirectory()
    {
        var baseDirectory     = Directory.GetCurrentDirectory();
        var scanDataDirectory = Path.Combine(baseDirectory, Filename);

        if (scanDataDirectory is null)
            throw new Exception("No ScanData Directory configured!");

        return scanDataDirectory;
    }
}