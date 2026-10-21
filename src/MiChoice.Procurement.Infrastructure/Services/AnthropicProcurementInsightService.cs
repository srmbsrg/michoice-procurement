using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MiChoice.Procurement.Infrastructure.Services;

/// <summary>
/// Live miAgentic insight service for miProcurement over the Anthropic Messages API, local-first:
/// by default it calls a free local model (Ollama at Anthropic:BaseUrl, model Anthropic:Model);
/// it calls Anthropic (claude-haiku-4-5-20251001 unless Anthropic:Model names another Claude model)
/// when Anthropic:ApiKey is set or Anthropic:BaseUrl points at api.anthropic.com.
/// Auto-activates when Anthropic:ApiKey or Anthropic:BaseUrl is configured.
/// Falls back to <see cref="StubAgenticInsightService"/> on any API failure.
/// </summary>
public class AnthropicProcurementInsightService : IAgenticInsightService
{
    private readonly HttpClient _http;
    private readonly string     _apiKey;
    private readonly StubAgenticInsightService _stub = new();
    private readonly string     _model;
    private readonly string     _apiUrl;

    private const string SystemPrompt =
        "You are miAgentic, an AI assistant for K-12 school district food service procurement. " +
        "Provide 1-2 sentence, actionable insights for nutrition directors and warehouse managers. " +
        "Focus on USDA NSLP program compliance, cost efficiency, and supply chain continuity. " +
        "Be direct and specific. Avoid filler language.";

    /// <param name="apiKey">Anthropic API key; may be empty for a local model server.</param>
    /// <param name="baseUrl">Anthropic:BaseUrl (e.g. http://localhost:11434); null/empty = api.anthropic.com.</param>
    /// <param name="model">Anthropic:Model; null/empty = <see cref="DefaultAnthropicModel"/>.</param>
    public AnthropicProcurementInsightService(IHttpClientFactory httpClientFactory, string? apiKey,
        string? baseUrl = null, string? model = null)
    {
        _http   = httpClientFactory.CreateClient("anthropic");
        _apiKey = apiKey ?? string.Empty;
        var (resolvedBaseUrl, resolvedModel) = ResolveEndpoint(apiKey, baseUrl, model);
        _model  = resolvedModel;
        _apiUrl = $"{resolvedBaseUrl}/v1/messages";
    }
    // ── Local-first endpoint resolution ───────────────────────────────────────
    // Config: Anthropic:BaseUrl, Anthropic:Model, Anthropic:ApiKey. The shipped
    // appsettings point at a local Ollama server (free, on the district's own box),
    // which serves the same Anthropic Messages API at {BaseUrl}/v1/messages.
    // A deployment that sets Anthropic:ApiKey and leaves BaseUrl empty or at a
    // loopback default keeps calling api.anthropic.com with a Claude model,
    // exactly as before.

    /// <summary>Public Anthropic API base URL (used when no other endpoint applies).</summary>
    public const string AnthropicCloudBaseUrl = "https://api.anthropic.com";

    /// <summary>Model used against Anthropic when Anthropic:Model is not a Claude model.</summary>
    public const string DefaultAnthropicModel = "claude-haiku-4-5-20251001";

    /// <summary>
    /// Resolves the Messages API base URL (no trailing slash) and model name.
    /// </summary>
    public static (string BaseUrl, string Model) ResolveEndpoint(
        string? apiKey, string? baseUrl, string? model)
    {
        var url = string.IsNullOrWhiteSpace(baseUrl)
            ? AnthropicCloudBaseUrl
            : baseUrl.Trim().TrimEnd('/');

        // Malformed URL => cloud default (only reachable with a key: IsConfigured is false
        // without one, so nothing is sent anywhere). Keyed deployment + loopback default
        // URL => real Anthropic (unchanged behaviour for keyed deployments).
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)
            || (!string.IsNullOrWhiteSpace(apiKey) && uri.IsLoopback))
            url = AnthropicCloudBaseUrl;

        var isAnthropicCloud = url.Contains("api.anthropic.com", StringComparison.OrdinalIgnoreCase);
        var resolvedModel = string.IsNullOrWhiteSpace(model)
                            || (isAnthropicCloud && !model.Trim().StartsWith("claude", StringComparison.OrdinalIgnoreCase))
            ? DefaultAnthropicModel
            : model.Trim();

        return (url, resolvedModel);
    }

    /// <summary>True when the real (non-stub) insight service should be registered.</summary>
    public static bool IsConfigured(string? apiKey, string? baseUrl)
        => !string.IsNullOrWhiteSpace(apiKey)
           || Uri.TryCreate(baseUrl?.Trim(), UriKind.Absolute, out _);

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

    public async Task<AgenticInsight?> GetReceivingDiscrepancyInsightAsync(
        int totalPos, int discrepancyPos, decimal orderedValue, decimal receivedValue,
        string topDiscrepancies, CancellationToken ct = default)
    {
        try
        {
            var variancePct = orderedValue > 0 ? (orderedValue - receivedValue) * 100m / orderedValue : 0m;
            var prompt =
                $"Receiving discrepancy analysis for K-12 school food service: " +
                $"{discrepancyPos} of {totalPos} received POs have short shipments. " +
                $"Total ordered: ${orderedValue:N0}. Total received: ${receivedValue:N0}. " +
                $"Variance: ${orderedValue - receivedValue:N0} ({variancePct:F1}%). " +
                (string.IsNullOrWhiteSpace(topDiscrepancies) ? "" : $"Primary discrepancy sources: {topDiscrepancies}. ") +
                "Provide a 1-2 sentence assessment of vendor fulfillment reliability and one specific " +
                "action recommendation for a K-12 food service procurement manager " +
                "(e.g., vendor follow-up, receiving checklist update, or order adjustment strategy).";
            var text = await CallAsync(prompt, ct);
            return new AgenticInsight(text, "ReceivingDiscrepancy", DateTimeOffset.UtcNow);
        }
        catch { return await _stub.GetReceivingDiscrepancyInsightAsync(totalPos, discrepancyPos, orderedValue, receivedValue, topDiscrepancies, ct); }
    }

    public async Task<AgenticInsight?> GetVendorSpendingInsightAsync(
        int activeVendors, decimal totalSpend, string topVendor, decimal topVendorSpend,
        decimal topVendorSharePct, CancellationToken ct = default)
    {
        try
        {
            var prompt =
                $"K-12 school district vendor spending analysis for NSLP procurement: " +
                $"{activeVendors} active vendor{(activeVendors == 1 ? "" : "s")} this school year. " +
                $"Total spend: ${totalSpend:N0}. " +
                $"Top vendor: {topVendor} at ${topVendorSpend:N0} ({topVendorSharePct:F1}% of spend). " +
                "Provide a 1-2 sentence assessment of vendor portfolio concentration risk and one action " +
                "recommendation to support supply chain resilience for a K-12 food service director " +
                "(consider USDA procurement regulations and NSLP continuity requirements).";
            var text = await CallAsync(prompt, ct);
            return new AgenticInsight(text, "VendorSpending", DateTimeOffset.UtcNow);
        }
        catch { return await _stub.GetVendorSpendingInsightAsync(activeVendors, totalSpend, topVendor, topVendorSpend, topVendorSharePct, ct); }
    }

    // ── Anthropic API helper ──────────────────────────────────────────────────

    private async Task<string> CallAsync(string userPrompt, CancellationToken ct)
    {
        var payload = new
        {
            model      = _model,
            max_tokens = 200,
            system     = SystemPrompt,
            messages   = new[] { new { role = "user", content = userPrompt } }
        };

        var body = JsonSerializer.Serialize(payload);
        using var request = new HttpRequestMessage(HttpMethod.Post, _apiUrl);
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
