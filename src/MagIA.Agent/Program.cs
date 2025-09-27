using Microsoft.Agents.CopilotStudio.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.Copilot;

IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("settings.json", optional: false, reloadOnChange: true)
    .Build();

IConfigurationSection copilotSection = configuration.GetSection("Copilot");

string tenantId = GetRequiredConfigValue(copilotSection, "TenantId");
string appClientId = GetRequiredConfigValue(copilotSection, "AppClientId");
string clientSecret = GetRequiredConfigValue(copilotSection, "ClientSecret");
string environmentId = GetRequiredConfigValue(copilotSection, "EnvironmentId");
string schemaName = GetRequiredConfigValue(copilotSection, "SchemaName");

CopilotStudioConnectionSettings settings = new(tenantId, appClientId, clientSecret)
{
    EnvironmentId = environmentId,
    SchemaName = schemaName
};

CopilotClient copilotClient = CopilotStudioAgent.CreateClient(settings);

CopilotStudioAgent agent = new(copilotClient);

await foreach(ChatMessageContent message in agent.InvokeAsync("What is Abyx and what kind of products do you offer?"))
{
    Console.Write(message.Content);
}

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