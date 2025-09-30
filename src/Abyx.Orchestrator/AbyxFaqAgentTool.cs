using System.ComponentModel;
using System.Text;
using System.Text.Json;
using Microsoft.Agents.CopilotStudio.Client;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.Copilot;

namespace Abyx.Orchestrator;

public sealed class AbyxFaqAgentTool(CopilotStudioConnectionSettings copilotStudioConnectionSettings)
{
    private readonly CopilotStudioConnectionSettings _copilotStudioConnectionSettings = copilotStudioConnectionSettings;

    [KernelFunction("invoke_copilot_agent")]
    [Description("Answer Abyx FAQs using the official Abyx knowledge base.")]
    public async Task<string> InvokeAsync(
        [Description("User question about Abyx products, ethics, support, etc.")]
        string userQuery)
    {
        CopilotClient copilotClient = CopilotStudioAgent.CreateClient(_copilotStudioConnectionSettings);

        CopilotStudioAgent agent = new(copilotClient);

        StringBuilder answer = new();
        await foreach (ChatMessageContent message in agent.InvokeAsync(
            userQuery))
        {
            answer.AppendLine(message.Content);
        }

        var result = new
        {
            source = "CopilotStudioAgent (FAQ)",
            answer
        };

        return JsonSerializer.Serialize(result);
    }
}
