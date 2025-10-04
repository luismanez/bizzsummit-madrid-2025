using System.ComponentModel;
using System.Text.Json;
using Microsoft.Agents.AI.CopilotStudio;
using Microsoft.Agents.CopilotStudio.Client;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;

namespace Abyx.Orchestrator;

public sealed class AbyxFaqAgentTool(
    ConnectionSettings copilotStudioConnectionSettings,
    IHttpClientFactory httpClientFactory)
{
    private readonly ConnectionSettings _copilotStudioConnectionSettings = copilotStudioConnectionSettings;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    [Description("Answer Abyx FAQs using the official Abyx knowledge base.")]
    public async Task<string> InvokeAsync(
        [Description("User question about Abyx products, ethics, support, etc.")]
        string userQuery)
    {
        CopilotClient copilotClient =
            new(
                _copilotStudioConnectionSettings,
                _httpClientFactory,
                NullLogger.Instance,
                "CopilotStudioAgent");

        CopilotStudioAgent agent = new CopilotStudioAgent(copilotClient);

        var answer = await agent.RunAsync(userQuery);

        var result = new
        {
            source = "CopilotStudioAgent (FAQ)",
            answer = answer.Text
        };

        return JsonSerializer.Serialize(result);
    }

    public AITool AsAITool()
    {
        return AIFunctionFactory.Create(this.InvokeAsync);
    }
}
