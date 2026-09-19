using Microsoft.EntityFrameworkCore;
using MiChoice.Procurement.Infrastructure.Data;
using MiChoice.Procurement.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddProcurementInfrastructure(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MiChoice.Procurement.Infrastructure.Data.ProcurementDbContext>();
    // One shared district DB: migrate on SQL Server (proc schema); EnsureCreated for local SQLite dev.
    if ((app.Configuration["DatabaseProvider"] ?? "sqlite").Equals("sqlserver", StringComparison.OrdinalIgnoreCase))
        await db.Database.MigrateAsync();
    else
        await db.Database.EnsureCreatedAsync();
}
await DatabaseSeeder.SeedAsync(app.Services);

app.MapGet("/health", () => Results.Ok("Healthy"));
app.Run();