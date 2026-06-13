using MiChoice.Procurement.Domain.Enums;

namespace MiChoice.Procurement.Domain.Entities;
public class TransferOrder
{
    public int Id { get; set; }
    public int FromCampusId { get; set; }
    public Campus FromCampus { get; set; } = null!;
    public int ToCampusId { get; set; }
    public Campus ToCampus { get; set; } = null!;
    public TransferOrderStatus Status { get; set; } = TransferOrderStatus.Draft;
    public DateOnly OrderDate { get; set; }
    public DateOnly? ExpectedDelivery { get; set; }
    public string? Notes { get; set; }
    public string? RequestedByUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public ICollection<TransferOrderLine> Lines { get; set; } = new List<TransferOrderLine>();
}
