using AhAnalyticsPriceUpdater;
using AhAnalyticsPriceUpdater.Services;
using Microsoft.Extensions.Configuration;

var builder = new ConfigurationBuilder()
             .SetBasePath(AppContext.BaseDirectory)
             .AddJsonFile("appsettings.json", true, true);

IConfiguration configuration = builder.Build();
var            logger        = LogProvider.GetLogger();

var decrypter = new ScanDataDecrypter(logger, configuration);
var sheetService = new SpreadsheetService(decrypter, logger);

sheetService.UpdateSpreadsheet();