using MiChoice.Procurement.Domain.Enums;

namespace MiChoice.Procurement.Domain.Entities;
/// <summary>Maps to EZTask Inventory master catalog.</summary>
public class InventoryItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public InventoryItemClass ItemClass { get; set; } = InventoryItemClass.Stock;
    public string? Category { get; set; }
    public string? SubCategory { get; set; }
    public string UnitOfMeasure { get; set; } = "EA";
    public decimal ReorderPoint { get; set; }
    public decimal MaxStockLevel { get; set; }
    /// <summary>USDA per-unit value (EZTask USDA_Cost column). Populated for Commodity-class items.</summary>
    public decimal UsdaCost { get; set; }
    /// <summary>True when this item participates in a recipe/BOM (EZTask blnHasRecipe).</summary>
    public bool HasRecipe { get; set; }
    public bool IsActive { get; set; } = true;
    public int? DefaultVendorId { get; set; }
    public Vendor? DefaultVendor { get; set; }
    public ICollection<InventoryStock> Stocks { get; set; } = new List<InventoryStock>();
    public ICollection<InventoryTransaction> Transactions { get; set; } = new List<InventoryTransaction>();
}
