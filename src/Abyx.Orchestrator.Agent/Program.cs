using Microsoft.Extensions.Configuration;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using OpenAI;
using Microsoft.Extensions.DependencyInjection;
using Abyx.Orchestrator.Agent.Auth;
using Microsoft.Agents.CopilotStudio.Client;
using Abyx.Orchestrator;
using Microsoft.Extensions.AI;

static string GetRequiredConfigValue(IConfiguration configuration, string key)
{
    string? value = configuration[key];

    if (string.IsNullOrWhiteSpace(value))
    {
        string prefix = configuration is IConfigurationSection section && !string.IsNullOrEmpty(section.Path)
            ? section.Path
            : string.Empty;

        string fullKey = string.IsNullOrEmpty(prefix) ? key : $"{prefix}:{key}";

        throw new InvalidOperationException($"Configuration value '{fullKey}' is missing or empty.");
    }

    return value;
}

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("settings.json", optional: false, reloadOnChange: true)
    .Build();

IConfigurationSection copilotSection = configuration.GetSection("Copilot");

string tenantId = GetRequiredConfigValue(copilotSection, "TenantId");
string appClientId = GetRequiredConfigValue(copilotSection, "AppClientId");
string clientSecret = GetRequiredConfigValue(copilotSection, "ClientSecret");
string environmentId = GetRequiredConfigValue(copilotSection, "EnvironmentId");
string schemaName = GetRequiredConfigValue(copilotSection, "SchemaName");

IConfigurationSection azureOpenAISection = configuration.GetSection("AzureOpenAI");
var deploymentName = GetRequiredConfigValue(azureOpenAISection, "DeploymentName");
var endpoint = GetRequiredConfigValue(azureOpenAISection, "Endpoint");
var apiKey = GetRequiredConfigValue(azureOpenAISection, "ApiKey");

ServiceCollection services = new();

CopilotStudioTokenHandler tokenHandler = new(tenantId, appClientId);

services
    .AddSingleton(tokenHandler)
    .AddHttpClient("CopilotStudioAgent")
    .ConfigurePrimaryHttpMessageHandler<CopilotStudioTokenHandler>();

IHttpClientFactory httpClientFactory =
    services
        .BuildServiceProvider()
        .GetRequiredService<IHttpClientFactory>();

ConnectionSettings settings = new()
{
    EnvironmentId = environmentId,
    SchemaName = schemaName
};


const string AgentName = "Abyx Agent";
const string AgentInstructions = "You are Abyx Orchestrator. Choose the best function/tool:\n" +
        "- Use CopilotStudio.invoke_copilot_agent for pure FAQs about Abyx products, ethics (AEP), Sentinel, Eternum.\n" +
        "- Use AbyxAdvisor.recommend_abyx_solution when user expresses a goal or need (health/companionship/cognition/digital-immortality).\n" +
        "- Use AbyxPricingApi.get_product_prices to fetch pricing; optionally chain Calc.compute_yearly_cost.\n" +
        "- If no tool needed, respond directly. Keep answers concise and cite tool outputs when used.";

var abyxFaqAgentTool = new AbyxFaqAgentTool(settings, httpClientFactory);

AIAgent agent = new AzureOpenAIClient(
    new Uri(endpoint),
    new AzureCliCredential())
     .GetChatClient(deploymentName)
     .CreateAIAgent(
        AgentInstructions,
        AgentName,
        tools:
            [
                abyxFaqAgentTool.AsAITool(),
                new AbyxRecommenderTool().AsAITool(),
                new AbyxPricingApiTool().AsAITool(),
                new CalculatorTool().AsAITool(),
            ]);

while (true)
{
    Console.WriteLine("-------------------------------");
    Console.Write("> ");
    var query = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(query)) continue;

    Console.WriteLine(
        await agent.RunAsync(
            query));
    Console.WriteLine();
}

// var query = "What is Abyx Eternum and how does privacy work?";
// Console.WriteLine(
//         await agent.RunAsync(
//             query));

// What is Abyx Eternum and how does privacy work?
// We need post-surgery cellular repair with safety constraints.
// Price for AbyxMind and yearly cost for 25 seats.