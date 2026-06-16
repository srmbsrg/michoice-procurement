using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MiChoice.Procurement.Infrastructure.Services;

/// <summary>
/// Live miAgentic insight service for miProcurement — calls Claude Haiku via Anthropic Messages API.
/// Auto-activates when Anthropic:ApiKey is present in Railway / appsettings.
/// Falls back to <see cref="StubAgenticInsightService"/> on any API failure.
/// </summary>
public class AnthropicProcurementInsightService : IAgenticInsightService
{
    private readonly HttpClient _http;
    private readonly string     _apiKey;
    private readonly StubAgenticInsightService _stub = new();
    private const string Model  = "claude-haiku-4-5-20251001";
    private const string ApiUrl = "https://api.anthropic.com/v1/messages";

    private const string SystemPrompt =
        "You are miAgentic, an AI assistant for K-12 school district food service procurement. " +
        "Provide 1-2 sentence, actionable insights for nutrition directors and warehouse managers. " +
        "Focus on USDA NSLP program compliance, cost efficiency, and supply chain continuity. " +
        "Be direct and specific. Avoid filler language.";

    public AnthropicProcurementInsightService(IHttpClientFactory httpClientFactory, string apiKey)
    {
        _http   = httpClientFactory.CreateClient("anthropic");
        _apiKey = apiKey;
    }

    // ── Interface implementation ──────────────────────────────────────────────

    public async Task<AgenticInsight?> GetInventoryInsightAsync(
        int campusId, CancellationToken ct = default)
    {
        try
        {
            var prompt =
                $"K-12 food service inventory review for Campus {campusId}. " +
                "Items at or near reorder threshold need immediate attention. " +
                "Provide a 1-sentence stock health assessment and recommend the most important action " +
                "for the warehouse manager today.";
            var text = await CallAsync(prompt, ct);
            return new AgenticInsight(text, "Inventory", DateTimeOffset.UtcNow);
        }
        catch { return await _stub.GetInventoryInsightAsync(campusId, ct); }
    }

    public async Task<AgenticInsight?> GetEntitlementInsightAsync(
        int schoolYear, CancellationToken ct = default)
    {
        try
        {
            var syLabel = $"SY {schoolYear - 1}-{schoolYear % 100:D2}";
            var prompt =
                $"K-12 USDA entitlement management for {syLabel}. " +
                "Unspent entitlement at year-end is lost — it cannot roll over. " +
                "Recommend a 1-sentence strategy for ensuring the district maximizes its USDA commodity allocation " +
                "before the June 30 spend-down deadline.";
            var text = await CallAsync(prompt, ct);
            return new AgenticInsight(text, "Entitlement", DateTimeOffset.UtcNow);
        }
        catch { return await _stub.GetEntitlementInsightAsync(schoolYear, ct); }
    }

    public async Task<AgenticInsight?> GetWasteInsightAsync(
        int campusId, DateOnly week, CancellationToken ct = default)
    {
        try
        {
            var prompt =
                $"Food waste analysis for Campus {campusId}, week of {week:MMMM d, yyyy}. " +
                "USDA NSLP requires reasonable portion control and minimum waste practices. " +
                "Provide a 1-sentence waste reduction recommendation and identify the most common cause " +
                "of excess production in K-12 food service operations.";
            var text = await CallAsync(prompt, ct);
            return new AgenticInsight(text, "Waste", DateTimeOffset.UtcNow);
        }
        catch { return await _stub.GetWasteInsightAsync(campusId, week, ct); }
    }

    public async Task<AgenticInsight?> GetReorderInsightAsync(
        int campusId, CancellationToken ct = default)
    {
        try
        {
            var prompt =
                $"Inventory reorder analysis for Campus {campusId}. " +
                "Items below their reorder point risk production gaps that affect USDA NSLP meal service. " +
                "Provide a 1-sentence recommendation for preventing stockouts, prioritizing items that " +
                "cannot be substituted (proteins, grains for Type A compliance).";
            var text = await CallAsync(prompt, ct);
            return new AgenticInsight(text, "Reorder", DateTimeOffset.UtcNow);
        }
        catch { return await _stub.GetReorderInsightAsync(campusId, ct); }
    }

    public async Task<AgenticInsight?> GetTransferInsightAsync(
        CancellationToken ct = default)
    {
        try
        {
            var prompt =
                "Campus-to-campus inventory transfer analysis for a K-12 school district. " +
                "Transfers reduce spoilage and eliminate emergency purchases when one campus is low. " +
                "Recommend the general inventory balancing strategy that minimizes waste " +
                "while maintaining service continuity across all campuses.";
            var text = await CallAsync(prompt, ct);
            return new AgenticInsight(text, "Transfer", DateTimeOffset.UtcNow);
        }
        catch { return await _stub.GetTransferInsightAsync(ct); }
    }

    public async Task<AgenticInsight?> GetVendorInsightAsync(
        int total, int active, int usdaCount, CancellationToken ct = default)
    {
        try
        {
            var inactive = total - active;
            var prompt =
                $"Vendor management summary: {total} total vendors, {active} active, " +
                $"{usdaCount} USDA commodity suppliers, {inactive} inactive. " +
                "From a K-12 procurement best-practice perspective: provide a 1-sentence recommendation " +
                "about vendor portfolio health — consider diversity, backup suppliers, and USDA commodity " +
                "supplier requirements under 7 CFR Part 250.";
            var text = await CallAsync(prompt, ct);
            return new AgenticInsight(text, "Vendor", DateTimeOffset.UtcNow);
        }
        catch { return await _stub.GetVendorInsightAsync(total, active, usdaCount, ct); }
    }

    public async Task<AgenticInsight?> GetPurchaseOrderInsightAsync(
        int total, int draft, int submitted, int received, decimal totalValue,
        CancellationToken ct = default)
    {
        try
        {
            var pending = draft + submitted;
            var prompt =
                $"Purchase order portfolio for a K-12 food service district: {total} total POs, " +
                $"{draft} draft (not submitted), {submitted} submitted to vendors, " +
                $"{received} received, total portfolio value ${totalValue:N0}. " +
                "Provide a 1-sentence procurement readiness assessment: flag if pending orders risk " +
                "delivery gaps or if submission delays may affect meal service continuity.";
            var text = await CallAsync(prompt, ct);
            return new AgenticInsight(text, "PurchaseOrder", DateTimeOffset.UtcNow);
        }
        catch { return await _stub.GetPurchaseOrderInsightAsync(total, draft, submitted, received, totalValue, ct); }
    }

    public async Task<AgenticInsight?> GetLowStockInsightAsync(
        int lowStockItems, int outOfStock, int campusCount, int totalItems,
        string topUrgent, CancellationToken ct = default)
    {
        try
        {
            var prompt =
                $"Low stock alert for a K-12 school district: {lowStockItems} items at or below reorder point, " +
                $"{outOfStock} completely out of stock, across {campusCount} campus(es). " +
                $"Total active inventory catalog: {totalItems} items. " +
                (string.IsNullOrWhiteSpace(topUrgent) ? "" : $"Highest priority items: {topUrgent}. ") +
                "Provide a 1-2 sentence assessment: flag the out-of-stock severity and recommend " +
                "the most urgent purchasing action to prevent USDA NSLP meal service disruption.";
            var text = await CallAsync(prompt, ct);
            return new AgenticInsight(text, "LowStock", DateTimeOffset.UtcNow);
        }
        catch { return await _stub.GetLowStockInsightAsync(lowStockItems, outOfStock, campusCount, totalItems, topUrgent, ct); }
    }

    // ── Anthropic API helper ──────────────────────────────────────────────────

    private async Task<string> CallAsync(string userPrompt, CancellationToken ct)
    {
        var payload = new
        {
            model      = Model,
            max_tokens = 200,
            system     = SystemPrompt,
            messages   = new[] { new { role = "user", content = userPrompt } }
        };

        var body = JsonSerializer.Serialize(payload);
        using var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
        request.Headers.Add("x-api-key", _apiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");
        request.Content = new StringContent(body, Encoding.UTF8, "application/json");

        var response = await _http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString() ?? "No insight returned.";
    }
}
