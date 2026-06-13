namespace MiChoice.Procurement.Infrastructure.Services;

public record AgenticInsight(string Insight, string Category, DateTimeOffset GeneratedAt);

public interface IAgenticInsightService
{
    Task<AgenticInsight?> GetInventoryInsightAsync(int campusId, CancellationToken ct = default);
    Task<AgenticInsight?> GetEntitlementInsightAsync(int schoolYear, CancellationToken ct = default);
    Task<AgenticInsight?> GetWasteInsightAsync(int campusId, DateOnly week, CancellationToken ct = default);
    Task<AgenticInsight?> GetReorderInsightAsync(int campusId, CancellationToken ct = default);
}

public class StubAgenticInsightService : IAgenticInsightService
{
    public Task<AgenticInsight?> GetInventoryInsightAsync(int campusId, CancellationToken ct = default)
    {
        var insight = campusId switch
        {
            3 => "Beef patties at Campus 3 are 4 days from reorder point based on current depletion rate. Next delivery window is Tuesday.",
            2 => "Campus 2 inventory is healthy across all categories. Ground beef will reach reorder in 6 days — consider bundling with next week's GFS order.",
            _ => $"Campus {campusId} has 2 items trending toward reorder thresholds. Monitor ground beef and whole wheat bread depletion rates."
        };
        return Task.FromResult<AgenticInsight?>(new AgenticInsight(insight, "Inventory", DateTimeOffset.UtcNow));
    }

    public Task<AgenticInsight?> GetEntitlementInsightAsync(int schoolYear, CancellationToken ct = default)
    {
        var insight = $"You have $4,230 in unspent USDA entitlement with 47 days until year-end (SY {schoolYear - 1}-{schoolYear % 100:D2}). " +
                      "At current ordering pace you will leave $1,800 on the table. " +
                      "Recommend ordering canned fruit (A047) and frozen chicken (A256) to close the gap.";
        return Task.FromResult<AgenticInsight?>(new AgenticInsight(insight, "Entitlement", DateTimeOffset.UtcNow));
    }

    public Task<AgenticInsight?> GetWasteInsightAsync(int campusId, DateOnly week, CancellationToken ct = default)
    {
        var insight = $"Week of {week:MMM d}: waste at Campus {campusId} is 11% above district average. " +
                      "Thursday's vegetable medley had the highest waste ratio (34%). " +
                      "Consider reducing production by 15% next cycle.";
        return Task.FromResult<AgenticInsight?>(new AgenticInsight(insight, "Waste", DateTimeOffset.UtcNow));
    }

    public Task<AgenticInsight?> GetReorderInsightAsync(int campusId, CancellationToken ct = default)
    {
        var insight = "3 items are at or below reorder point: ground beef (Campus 2), whole wheat bread (Campuses 1 & 4), apple juice (Campus 3). " +
                      "Recommend consolidating into a single GFS order to qualify for volume pricing.";
        return Task.FromResult<AgenticInsight?>(new AgenticInsight(insight, "Reorder", DateTimeOffset.UtcNow));
    }
}
