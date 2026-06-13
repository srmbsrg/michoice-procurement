using MiChoice.Procurement.Domain.Enums;

namespace MiChoice.Procurement.Domain.Entities;
public class EntitlementOrder
{
    public int Id { get; set; }
    public int EntitlementId { get; set; }
    public Entitlement Entitlement { get; set; } = null!;
    public int CommodityItemId { get; set; }
    public CommodityItem CommodityItem { get; set; } = null!;
    public decimal QuantityOrdered { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost => QuantityOrdered * UnitCost;
    public EntitlementOrderStatus Status { get; set; } = EntitlementOrderStatus.Pending;
    public DateTimeOffset OrderedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ReceivedAt { get; set; }
    public string? Notes { get; set; }
}
