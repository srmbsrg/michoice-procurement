using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MiChoice.Procurement.Infrastructure.Data;

/// <summary>
/// Design-time factory for the EF Core CLI. Targets SQL Server — the shared district
/// database — scaffolding SQL-Server-shaped migrations into the "proc" schema with
/// its own history table. Not used at runtime. Override target with env
/// MiChoice__Database__ConnectionString. (2026-09-19, one-DB convergence.)
/// </summary>
public class ProcurementDbContextDesignFactory : IDesignTimeDbContextFactory<ProcurementDbContext>
{
    public ProcurementDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("MiChoice__Database__ConnectionString")
            ?? "Server=.;Database=MiChoiceDistrict;Trusted_Connection=True;TrustServerCertificate=True;";

        var options = new DbContextOptionsBuilder<ProcurementDbContext>()
            .UseSqlServer(connectionString,
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "proc"))
            .Options;

        return new ProcurementDbContext(options);
    }
}
