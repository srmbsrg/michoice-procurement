namespace MiChoice.Procurement.Domain.Entities;
public class TransferOrderLine
{
    public int Id { get; set; }
    public int TransferOrderId { get; set; }
    public TransferOrder TransferOrder { get; set; } = null!;
    public int InventoryItemId { get; set; }
    public InventoryItem InventoryItem { get; set; } = null!;
    public decimal QuantityRequested { get; set; }
    public decimal QuantityShipped { get; set; }
    public decimal QuantityReceived { get; set; }
    public string Unit { get; set; } = "CS";
    public string? Notes { get; set; }
}
