namespace MiChoice.Procurement.Infrastructure.Services;

public record AgenticInsight(string Insight, string Category, DateTimeOffset GeneratedAt);

public interface IAgenticInsightService
{
    Task<AgenticInsight?> GetInventoryInsightAsync(int campusId, CancellationToken ct = default);
    Task<AgenticInsight?> GetEntitlementInsightAsync(int schoolYear, CancellationToken ct = default);
    Task<AgenticInsight?> GetWasteInsightAsync(int campusId, DateOnly week, CancellationToken ct = default);
    Task<AgenticInsight?> GetReorderInsightAsync(int campusId, CancellationToken ct = default);
    Task<AgenticInsight?> GetTransferInsightAsync(CancellationToken ct = default);
    Task<AgenticInsight?> GetVendorInsightAsync(int total, int active, int usdaCount, CancellationToken ct = default);
    Task<AgenticInsight?> GetPurchaseOrderInsightAsync(int total, int draft, int submitted, int received, decimal totalValue, CancellationToken ct = default);
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

    public Task<AgenticInsight?> GetTransferInsightAsync(CancellationToken ct = default)
    {
        var insight = "Inventory imbalance detected across campuses. Campus 2 holds 140% of reorder level for ground beef while Campus 3 is below threshold. " +
                      "A transfer of 4 CS ground beef from Campus 2 to Campus 3 would balance stock and reduce spoilage risk. " +
                      "Also consider moving excess whole wheat bread from Campus 1 to Campus 4, which has only 2-day supply remaining.";
        return Task.FromResult<AgenticInsight?>(new AgenticInsight(insight, "Transfer", DateTimeOffset.UtcNow));
    }

    public Task<AgenticInsight?> GetVendorInsightAsync(int total, int active, int usdaCount, CancellationToken ct = default)
    {
        var inactive = total - active;
        string insight;
        if (total == 0)
        {
            insight = "No vendors on file yet. Add your first supplier to start tracking procurement relationships and build purchasing history.";
        }
        else
        {
            var parts = new System.Text.StringBuilder();
            parts.Append($"You have {active} active vendor{(active == 1 ? "" : "s")}");
            if (usdaCount > 0)
                parts.Append($" including {usdaCount} USDA commodity supplier{(usdaCount == 1 ? "" : "s")}");
            parts.Append(". ");
            if (inactive > 0)
                parts.Append($"{inactive} vendor{(inactive == 1 ? " is" : "s are")} inactive — review quarterly to remove stale records. ");
            parts.Append("Consolidating to fewer primary suppliers typically improves pricing leverage and simplifies receiving logistics.");
            insight = parts.ToString();
        }
        return Task.FromResult<AgenticInsight?>(new AgenticInsight(insight, "Vendor", DateTimeOffset.UtcNow));
    }

    public Task<AgenticInsight?> GetPurchaseOrderInsightAsync(int total, int draft, int submitted, int received, decimal totalValue, CancellationToken ct = default)
    {
        string insight;
        if (total == 0)
        {
            insight = "No purchase orders on file yet. Create your first PO to begin tracking vendor orders and building procurement history.";
        }
        else if (submitted > 0 && draft > 0)
        {
            insight = $"{submitted} PO{(submitted == 1 ? "" : "s")} currently submitted to vendors and {draft} draft{(draft == 1 ? "" : "s")} awaiting final submission. " +
                      $"Total portfolio value: ${totalValue:N0}. Submit pending drafts to maintain order timelines and avoid supply gaps.";
        }
        else if (submitted > 0)
        {
            insight = $"{submitted} PO{(submitted == 1 ? "" : "s")} submitted to vendors totaling ${totalValue:N0} across {total} order{(total == 1 ? "" : "s")}. " +
                      "Follow up with vendors on any orders approaching or past expected delivery dates to keep receiving on schedule.";
        }
        else if (draft > 0)
        {
            insight = $"{draft} draft PO{(draft == 1 ? "" : "s")} awaiting submission. Total portfolio: ${totalValue:N0} across {total} order{(total == 1 ? "" : "s")}. " +
                      "Submit drafts promptly to avoid procurement delays and ensure timely delivery.";
        }
        else
        {
            insight = $"All {received} order{(received == 1 ? "" : "s")} received — no open procurement actions. " +
                      $"Historical spend: ${totalValue:N0}. Review vendor performance before the next ordering cycle to identify consolidation opportunities.";
        }
        return Task.FromResult<AgenticInsight?>(new AgenticInsight(insight, "PurchaseOrder", DateTimeOffset.UtcNow));
    }
}
