using MiChoice.Procurement.Domain.Enums;

namespace MiChoice.Procurement.Domain.Entities;
/// <summary>USDA commodity catalog entry. Extends InventoryItem for commodity-class items.</summary>
public class CommodityItem
{
    public int Id { get; set; }
    public string UsdaCatalogCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string UnitOfMeasure { get; set; } = "CS";
    public decimal EstimatedUnitCost { get; set; }
    public CommodityCategory Category { get; set; } = CommodityCategory.Other;
    public bool IsActive { get; set; } = true;
    public ICollection<EntitlementOrder> EntitlementOrders { get; set; } = new List<EntitlementOrder>();
}
