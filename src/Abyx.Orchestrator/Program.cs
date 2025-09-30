
using Abyx.Orchestrator;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.Copilot;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

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

CopilotStudioConnectionSettings settings = new(tenantId, appClientId, clientSecret)
{
    EnvironmentId = environmentId,
    SchemaName = schemaName
};

var kernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(
        deploymentName: deploymentName!,
        endpoint: endpoint!,
        apiKey: apiKey!)
    .Build();

kernel.ImportPluginFromObject(new AbyxFaqAgentTool(settings));
kernel.ImportPluginFromObject(new AbyxPricingApiTool());
kernel.ImportPluginFromObject(new AbyxRecommenderTool());
kernel.ImportPluginFromObject(new CalculatorTool());

var chat = kernel.GetRequiredService<IChatCompletionService>();
var history = new ChatHistory();

Console.WriteLine("Abyx Orchestrator ready. Ask me anything.\n");

while (true)
{
    Console.Write("> ");
    var user = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(user)) continue;

    history.AddUserMessage(user);

    history.AddSystemMessage(
        "You are Abyx Orchestrator. Choose the best function/tool:\n" +
        "- Use CopilotStudio.invoke_copilot_agent for pure FAQs about Abyx products, ethics (AEP), Sentinel, Eternum.\n" +
        "- Use AbyxAdvisor.recommend_abyx_solution when user expresses a goal or need (health/companionship/cognition/digital-immortality).\n" +
        "- Use AbyxPricingApi.get_product_prices to fetch pricing; optionally chain Calc.compute_yearly_cost.\n" +
        "- If no tool needed, respond directly. Keep answers concise and cite tool outputs when used."
    );

    var promptExecutionSettings = new OpenAIPromptExecutionSettings
    {
        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
    };

    var response = await chat.GetChatMessageContentAsync(history, promptExecutionSettings, kernel);
    Console.WriteLine($"\n{response.Role}: {response.Content}\n");
    history.AddMessage(response.Role, response.Content ?? string.Empty);
}
