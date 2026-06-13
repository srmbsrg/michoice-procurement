using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MiChoice.Procurement.Domain.Entities;
using MiChoice.Procurement.Infrastructure.Identity;

namespace MiChoice.Procurement.Infrastructure.Data;

public class ProcurementDbContext : IdentityDbContext<ProcurementUser>
{
    public ProcurementDbContext(DbContextOptions<ProcurementDbContext> options) : base(options) { }

    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<Campus> Campuses => Set<Campus>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<InventoryStock> InventoryStocks => Set<InventoryStock>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
    public DbSet<CommodityItem> CommodityItems => Set<CommodityItem>();
    public DbSet<Entitlement> Entitlements => Set<Entitlement>();
    public DbSet<EntitlementOrder> EntitlementOrders => Set<EntitlementOrder>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();
    public DbSet<TransferOrder> TransferOrders => Set<TransferOrder>();
    public DbSet<TransferOrderLine> TransferOrderLines => Set<TransferOrderLine>();
    public DbSet<WasteLog> WasteLogs => Set<WasteLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Entitlement>()
            .Ignore(e => e.UsedDollars)
            .Ignore(e => e.RemainingDollars);

        builder.Entity<PurchaseOrder>()
            .Ignore(po => po.TotalCost);

        builder.Entity<PurchaseOrderLine>()
            .Ignore(l => l.LineTotal)
            .Ignore(l => l.IsReceived);

        builder.Entity<EntitlementOrder>()
            .Ignore(eo => eo.TotalCost);

        builder.Entity<WasteLog>()
            .Ignore(w => w.WasteQuantity)
            .Ignore(w => w.WastePercent);

        builder.Entity<InventoryStock>()
            .HasIndex(s => new { s.InventoryItemId, s.CampusId })
            .IsUnique();

        builder.Entity<TransferOrder>()
            .HasOne(t => t.FromCampus)
            .WithMany()
            .HasForeignKey(t => t.FromCampusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<TransferOrder>()
            .HasOne(t => t.ToCampus)
            .WithMany()
            .HasForeignKey(t => t.ToCampusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
