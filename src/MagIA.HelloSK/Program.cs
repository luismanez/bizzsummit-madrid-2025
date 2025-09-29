using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;

// Build configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("settings.json", optional: false, reloadOnChange: true)
    .Build();

// Get Azure OpenAI settings
var deploymentName = configuration["AzureOpenAI:DeploymentName"];
var endpoint = configuration["AzureOpenAI:Endpoint"];
var apiKey = configuration["AzureOpenAI:ApiKey"];

var kernel = Kernel.CreateBuilder()
.AddAzureOpenAIChatClient(
    deploymentName: deploymentName!,
    endpoint: endpoint!,
    apiKey: apiKey!)
.Build();

Console.WriteLine(await kernel.InvokePromptAsync(
    "Who founded Microsoft? and when?"));
Console.WriteLine();