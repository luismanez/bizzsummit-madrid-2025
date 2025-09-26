using Microsoft.Agents.CopilotStudio.Client;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.Copilot;

string appClientId = "<your-app-client-id>";
string tenantId = "<your-tenant-id>";
string clientSecret = "<your-client-secret>";

CopilotStudioConnectionSettings settings = new(tenantId, appClientId, clientSecret);
settings.EnvironmentId = "agent-env-id";
settings.SchemaName = "agent-id";

CopilotClient copilotClient = CopilotStudioAgent.CreateClient(settings);

CopilotStudioAgent agent = new(copilotClient);

await foreach(ChatMessageContent message in agent.InvokeAsync("Hello, how are you?"))
{
    Console.Write(message.Content);
}