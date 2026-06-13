using Microsoft.AspNetCore.Identity;

namespace MiChoice.Procurement.Infrastructure.Identity;

public class ProcurementUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int? CampusId { get; set; }
    public string Role { get; set; } = "Staff";
    public string FullName => $"{FirstName} {LastName}".Trim();
}
