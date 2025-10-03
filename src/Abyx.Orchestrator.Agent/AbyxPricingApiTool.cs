using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.AI;

namespace Abyx.Orchestrator;

public sealed class AbyxPricingApiTool
{
    private static readonly Dictionary<string, (string Plan, double EurPerMonth)> Catalog = new(StringComparer.OrdinalIgnoreCase)
    {
        ["AbyxNano"] = ("Therapy", 299),
        ["AbyxHumanoids"] = ("Companion", 999),
        ["AbyxMind"] = ("Cognition+", 149),
        ["AbyxEternum"] = ("EternumCore", 499)
    };

    [Description("Get Abyx product pricing. Products: AbyxNano, AbyxHumanoids, AbyxMind, AbyxEternum.")]
    public Task<string> GetProductPricesAsync(
        [Description("Abyx product name (e.g., AbyxMind)")]
        string productName)
    {
        if (!Catalog.TryGetValue(productName, out var info))
        {
            var notFound = new { source = "AbyxPricingApi", product = productName, found = false };
            return Task.FromResult(JsonSerializer.Serialize(notFound));
        }

        var result = new
        {
            source = "AbyxPricingApi",
            product = productName,
            plans = new[] { new { plan = info.Plan, eurPerMonth = info.EurPerMonth } }
        };

        return Task.FromResult(JsonSerializer.Serialize(result));
    }

    public AITool AsAITool()
    {
        return AIFunctionFactory.Create(GetProductPricesAsync);
    }
}
