namespace AhAnalyticsPriceUpdater.Models;

public class CellMarketItem
{
    public string?  MarketItem          { get; set; }
    public decimal Price               { get; set; }
    public string  CellAddressToUpdate { get; set; }
}