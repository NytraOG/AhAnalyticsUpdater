namespace AhAnalyticsPriceUpdater.Models;

public class CellMaterial
{
    public string?  Material            { get; set; }
    public decimal Price               { get; set; }
    public string  CellAddressToUpdate { get; set; }
}