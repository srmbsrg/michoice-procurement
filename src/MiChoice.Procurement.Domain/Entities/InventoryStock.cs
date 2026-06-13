namespace MiChoice.Procurement.Domain.Entities;
/// <summary>Per-campus current stock level. Derived from InventoryTransactions but cached for performance.</summary>
public class InventoryStock
{
    public int Id { get; set; }
    public int InventoryItemId { get; set; }
    public InventoryItem InventoryItem { get; set; } = null!;
    public int CampusId { get; set; }
    public Campus Campus { get; set; } = null!;
    public decimal CurrentStock { get; set; }
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
}
