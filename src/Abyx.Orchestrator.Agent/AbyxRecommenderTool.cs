using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.AI;

namespace Abyx.Orchestrator;

public sealed class AbyxRecommenderTool
{
    [Description("Recommend the best Abyx solution given a user goal: health, companionship, cognition, digital-immortality.")]
    public Task<string> RecommendAsync(
        [Description("User high-level goal (health|companionship|cognition|digital-immortality)")]
        string goal,
        [Description("Optional context like priority, constraints, notes.")]
        string context = "")
    {
        var g = goal.Trim().ToLowerInvariant();
        var rec = g switch
        {
            "health" => new { product = "AbyxNano", pitch = "Regenerative medicine & targeted delivery with self-deactivation." },
            "companionship" => new { product = "AbyxHumanoids", pitch = "Lifelike companions with empathic AI and adaptive skin." },
            "cognition" => new { product = "AbyxMind", pitch = "AI models that extend human cognition and decision-making." },
            "digital-immortality" => new { product = "AbyxEternum", pitch = "Encrypted cognitive backups restorable to humanoids or virtual envs." },
            _ => new { product = "AbyxMind", pitch = "Default: augment cognition with safe boundaries (AEP)." }
        };

        var result = new
        {
            source = "AbyxRecommender",
            goal = g,
            recommendation = rec.product,
            rationale = rec.pitch,
            context
        };

        return Task.FromResult(JsonSerializer.Serialize(result));
        //return Task.FromResult(rec.product);
    }
    
    public AITool AsAITool()
    {
        return AIFunctionFactory.Create(this.RecommendAsync);
    }
}
