namespace MiChoice.Procurement.Domain.Entities;
/// <summary>Maps to EZTask Shipping_Locations. A school site or central warehouse.</summary>
public class Campus
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? Phone { get; set; }
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Zip { get; set; }
    public bool IsHub { get; set; }
    public bool IsActive { get; set; } = true;
}
