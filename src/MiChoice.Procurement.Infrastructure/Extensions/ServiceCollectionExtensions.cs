using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiChoice.Procurement.Infrastructure.Data;
using MiChoice.Procurement.Infrastructure.Identity;
using MiChoice.Procurement.Infrastructure.Services;

namespace MiChoice.Procurement.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProcurementInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var provider = configuration["DatabaseProvider"] ?? "sqlite";

        if (provider.Equals("sqlserver", StringComparison.OrdinalIgnoreCase))
        {
            services.AddDbContext<ProcurementDbContext>(opts =>
                opts.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        }
        else
        {
            services.AddDbContext<ProcurementDbContext>(opts =>
                opts.UseSqlite(configuration.GetConnectionString("DefaultConnection") ?? "Data Source=procurement.db"));
        }

        services.AddDefaultIdentity<ProcurementUser>(opts =>
        {
            opts.Password.RequireDigit = false;
            opts.Password.RequireUppercase = false;
            opts.Password.RequiredLength = 6;
            opts.SignIn.RequireConfirmedAccount = false;
        })
        .AddEntityFrameworkStores<ProcurementDbContext>();

        // Local-first: live service when Anthropic:ApiKey or a model-server Anthropic:BaseUrl
        // (default: local Ollama) is configured, stub otherwise. The live service falls back
        // to the stub on any call failure (e.g. Ollama not installed).
        var anthropicKey = configuration["Anthropic:ApiKey"];
        var anthropicBaseUrl = configuration["Anthropic:BaseUrl"];
        var anthropicModel = configuration["Anthropic:Model"];
        if (AnthropicProcurementInsightService.IsConfigured(anthropicKey, anthropicBaseUrl))
        {
            services.AddHttpClient("anthropic");
            services.AddScoped<IAgenticInsightService>(sp =>
                new AnthropicProcurementInsightService(
                    sp.GetRequiredService<IHttpClientFactory>(), anthropicKey,
                    anthropicBaseUrl, anthropicModel));
        }
        else
        {
            services.AddScoped<IAgenticInsightService, StubAgenticInsightService>();
        }

        return services;
    }
}
