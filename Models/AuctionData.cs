namespace AhAnalyticsPriceUpdater.Models;

public class AuctionData
{
    public string? ItemName       { get; set; }
    public string? Seller         { get; set; }
    public int     StackSize      { get; set; }
    public int     MinLvl         { get; set; }
    public int     BuyoutInCopper { get; set; }
    public decimal BuyoutInSilver => (decimal)BuyoutInCopper / 100;
    public decimal BuyoutInGold   => BuyoutInSilver / 100;
}