using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MiChoice.Procurement.Domain.Entities;
using MiChoice.Procurement.Domain.Enums;

namespace MiChoice.Procurement.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ProcurementDbContext>();
        await db.Database.MigrateAsync();

        if (await db.Vendors.AnyAsync()) return;

        var campuses = new[]
        {
            new Campus { Name = "District Warehouse", IsHub = true, City = "Springfield", State = "IL", IsActive = true },
            new Campus { Name = "Jefferson Elementary", IsHub = false, City = "Springfield", State = "IL", IsActive = true },
            new Campus { Name = "Lincoln Middle School", IsHub = false, City = "Springfield", State = "IL", IsActive = true },
            new Campus { Name = "Roosevelt High School", IsHub = false, City = "Springfield", State = "IL", IsActive = true },
            new Campus { Name = "Washington Elementary", IsHub = false, City = "Springfield", State = "IL", IsActive = true },
        };
        db.Campuses.AddRange(campuses);

        var vendors = new[]
        {
            new Vendor { Name = "Sysco Foods", ContactName = "Maria Gonzalez", Phone = "217-555-0101", Email = "mgonzalez@sysco.example", IsUsdaCommoditySupplier = false, IsActive = true },
            new Vendor { Name = "Gordon Food Service", ContactName = "James Hendricks", Phone = "217-555-0102", Email = "jhendricks@gfs.example", IsUsdaCommoditySupplier = false, IsActive = true },
            new Vendor { Name = "USDA Commodity Program", ContactName = "USDA State Agency", Phone = "800-555-0100", Email = "commodities@usda.example", IsUsdaCommoditySupplier = true, IsActive = true },
            new Vendor { Name = "Fresh Farms Produce", ContactName = "Carlos Rivera", Phone = "217-555-0103", Email = "crivera@freshfarms.example", IsUsdaCommoditySupplier = false, IsActive = true },
            new Vendor { Name = "Prairie Dairy Co.", ContactName = "Susan Park", Phone = "217-555-0104", Email = "spark@prairiedairy.example", IsUsdaCommoditySupplier = false, IsActive = true },
        };
        db.Vendors.AddRange(vendors);

        var commodities = new[]
        {
            new CommodityItem { UsdaCatalogCode = "A099", Description = "Beef, Ground, Frozen, 80/20", UnitOfMeasure = "LB", EstimatedUnitCost = 2.85m, Category = CommodityCategory.Protein },
            new CommodityItem { UsdaCatalogCode = "A256", Description = "Chicken, Whole, Frozen", UnitOfMeasure = "LB", EstimatedUnitCost = 1.95m, Category = CommodityCategory.Protein },
            new CommodityItem { UsdaCatalogCode = "B047", Description = "Peaches, Canned, Sliced", UnitOfMeasure = "CS", EstimatedUnitCost = 18.50m, Category = CommodityCategory.Fruit },
            new CommodityItem { UsdaCatalogCode = "B112", Description = "Corn, Canned, Whole Kernel", UnitOfMeasure = "CS", EstimatedUnitCost = 16.75m, Category = CommodityCategory.Vegetable },
            new CommodityItem { UsdaCatalogCode = "C033", Description = "Apple Juice, 4 oz Boxes", UnitOfMeasure = "CS", EstimatedUnitCost = 22.40m, Category = CommodityCategory.Fruit },
            new CommodityItem { UsdaCatalogCode = "C088", Description = "Peanut Butter, Smooth", UnitOfMeasure = "CS", EstimatedUnitCost = 31.20m, Category = CommodityCategory.Protein },
            new CommodityItem { UsdaCatalogCode = "D014", Description = "Mozzarella Cheese, Shredded", UnitOfMeasure = "LB", EstimatedUnitCost = 3.40m, Category = CommodityCategory.Dairy },
            new CommodityItem { UsdaCatalogCode = "D062", Description = "Butter, Unsalted", UnitOfMeasure = "LB", EstimatedUnitCost = 2.10m, Category = CommodityCategory.Dairy },
            new CommodityItem { UsdaCatalogCode = "E019", Description = "Flour, Whole Wheat", UnitOfMeasure = "LB", EstimatedUnitCost = 0.65m, Category = CommodityCategory.Grain },
            new CommodityItem { UsdaCatalogCode = "E078", Description = "Brown Rice, Long Grain", UnitOfMeasure = "LB", EstimatedUnitCost = 0.48m, Category = CommodityCategory.Grain },
        };
        db.CommodityItems.AddRange(commodities);

        var currentYear = DateTime.Today.Month >= 7 ? DateTime.Today.Year + 1 : DateTime.Today.Year;
        var entitlement = new Entitlement
        {
            SchoolYear = currentYear,
            TotalDollars = 42_500m,
            Notes = $"SY {currentYear - 1}-{currentYear % 100:D2} USDA entitlement allocation"
        };
        db.Entitlements.Add(entitlement);

        await db.SaveChangesAsync();
    }
}
