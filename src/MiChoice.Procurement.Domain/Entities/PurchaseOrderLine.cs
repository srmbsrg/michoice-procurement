namespace MiChoice.Procurement.Domain.Entities;
/// <summary>Maps to EZTask PO_Lineitems. Includes Inv_UOM_Qty and Recipe_UOM_Qty for unit conversion.</summary>
public class PurchaseOrderLine
{
    public int Id { get; set; }
    public int PurchaseOrderId { get; set; }
    public PurchaseOrder PurchaseOrder { get; set; } = null!;
    public int? InventoryItemId { get; set; }
    public InventoryItem? InventoryItem { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal QuantityOrdered { get; set; }
    public string Unit { get; set; } = "CS";
    public decimal UnitCost { get; set; }
    public decimal LineTotal => QuantityOrdered * UnitCost;
    public decimal QuantityReceived { get; set; }
    public bool IsReceived => QuantityReceived >= QuantityOrdered;
    public DateOnly? DateExpected { get; set; }
    public DateOnly? DateReceived { get; set; }
    /// <summary>EZTask Inv_UOM_Qty — units per case in inventory UOM</summary>
    public decimal InvUomQty { get; set; } = 1;
    /// <summary>EZTask Recipe_UOM_Qty — units per case in recipe/portion UOM</summary>
    public decimal RecipeUomQty { get; set; } = 1;
    public string? Comments { get; set; }
}
