namespace MiChoice.Procurement.Domain.Entities;
public class WasteLog
{
    public int Id { get; set; }
    public int CampusId { get; set; }
    public Campus Campus { get; set; } = null!;
    public DateOnly ServiceDate { get; set; }
    public string MenuItemName { get; set; } = string.Empty;
    public decimal ProducedQuantity { get; set; }
    public decimal ServedQuantity { get; set; }
    public decimal WasteQuantity => ProducedQuantity - ServedQuantity;
    public decimal WastePercent => ProducedQuantity > 0 ? WasteQuantity / ProducedQuantity * 100 : 0;
    public string? WasteReason { get; set; }
    public string? Notes { get; set; }
    public int? InventoryItemId { get; set; }
    public InventoryItem? InventoryItem { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
