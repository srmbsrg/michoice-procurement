using MiChoice.Procurement.Domain.Enums;

namespace MiChoice.Procurement.Domain.Entities;
/// <summary>Audit ledger for all stock movements. Maps to EZTask Transaction_History + Inventory_Products receipts.</summary>
public class InventoryTransaction
{
    public int Id { get; set; }
    public int InventoryItemId { get; set; }
    public InventoryItem InventoryItem { get; set; } = null!;
    public int CampusId { get; set; }
    public Campus Campus { get; set; } = null!;
    public InventoryTransactionType TransactionType { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public string? Notes { get; set; }
    public DateOnly TransactionDate { get; set; }
    /// <summary>FK to PurchaseOrder.Id, TransferOrder.Id, or WasteLog.Id depending on TransactionType.</summary>
    public int? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }
    public string? CreatedByUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
