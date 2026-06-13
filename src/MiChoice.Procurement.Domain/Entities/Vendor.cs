namespace MiChoice.Procurement.Domain.Entities;

public class Vendor
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? AccountNumber { get; set; }
    /// <summary>Mirrors EZTask Vendor_Type. True when supplier can fulfil USDA commodity orders.</summary>
    public bool IsUsdaCommoditySupplier { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
}
