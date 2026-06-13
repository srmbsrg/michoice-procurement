using MiChoice.Procurement.Domain.Enums;

namespace MiChoice.Procurement.Domain.Entities;
/// <summary>Maps to EZTask Orders table (Order_Type=1). Supports both local vendor and USDA commodity sourcing.</summary>
public class PurchaseOrder
{
    public int Id { get; set; }
    public int VendorId { get; set; }
    public Vendor Vendor { get; set; } = null!;
    public int CampusId { get; set; }
    public Campus Campus { get; set; } = null!;
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;
    public DateOnly OrderDate { get; set; }
    public DateOnly? ExpectedDelivery { get; set; }
    public string? Notes { get; set; }
    public string? OrderedByUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public ICollection<PurchaseOrderLine> Lines { get; set; } = new List<PurchaseOrderLine>();

    public decimal TotalCost => Lines.Sum(l => l.LineTotal);
}
