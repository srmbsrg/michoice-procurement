using MiChoice.Procurement.Domain.Enums;

namespace MiChoice.Procurement.Domain.Entities;
public class Entitlement
{
    public int Id { get; set; }
    public int SchoolYear { get; set; }
    public decimal TotalDollars { get; set; }
    public string? Notes { get; set; }
    public ICollection<EntitlementOrder> EntitlementOrders { get; set; } = new List<EntitlementOrder>();

    public decimal UsedDollars => EntitlementOrders
        .Where(o => o.Status != EntitlementOrderStatus.Cancelled)
        .Sum(o => o.TotalCost);
    public decimal RemainingDollars => TotalDollars - UsedDollars;
}
