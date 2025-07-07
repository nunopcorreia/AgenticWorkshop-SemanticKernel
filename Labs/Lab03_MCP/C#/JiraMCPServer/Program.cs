// Import packages
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;
using DotNetEnv;
using Microsoft.SemanticKernel.ChatCompletion;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Microsoft.Extensions.Telemetry.Console;



// Load environment variables from .env file
var root = Directory.GetCurrentDirectory();
var dotenv = Path.Combine(root, ".env");
Env.Load(dotenv);

// Populate values from your OpenAI deployment
var modelId = "gpt-4o";
var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? 
    throw new ArgumentNullException("AZURE_OPENAI_ENDPOINT environment variable is not set");
var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY") ?? 
    throw new ArgumentNullException("AZURE_OPENAI_KEY environment variable is not set");
var jiraApiToken = Environment.GetEnvironmentVariable("JIRA_API_TOKEN") ?? 
    throw new ArgumentNullException("JIRA_API_TOKEN environment variable is not set");
var jiraEmail = Environment.GetEnvironmentVariable("JIRA_EMAIL") ?? 
    throw new ArgumentNullException("JIRA_EMAIL environment variable is not set");
var jiraServerUrl = Environment.GetEnvironmentVariable("JIRA_SERVER_URL") ?? 
    throw new ArgumentNullException("JIRA_SERVER_URL environment variable is not set");

// Create an MCPClient for the Jira server
var clientTransport = new StdioClientTransport(new StdioClientTransportOptions
{
    Name = "MCPServer",
    Command = "npx",
    Arguments = ["-y", "@modelcontextprotocol/server-jira"],
    EnvironmentVariables =  new() 
    { 
        { "JIRA_API_TOKEN", jiraApiToken },
        { "JIRA_EMAIL", jiraEmail },
        { "JIRA_SERVER_URL", jiraServerUrl }
    },
});

var mcpClient = await McpClientFactory.CreateAsync(clientTransport);

// Retrieve the list of tools available on the Jira server
var tools = await mcpClient.ListToolsAsync().ConfigureAwait(false);
foreach (var tool in tools)
{
    Console.WriteLine($"{tool.Name}: {tool.Description}");
}

// Prepare and build kernel with the MCP tools as Kernel functions
// Create a kernel with Azure OpenAI chat completion
//Full list of Supported Connectors: https://learn.microsoft.com/en-us/semantic-kernel/get-started/supported-languages?pivots=programming-language-csharp
var builder = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(modelId, endpoint, apiKey);

builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Warning));

Kernel kernel = builder.Build();
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
kernel.Plugins.AddFromFunctions("Jira", tools.Select(aiFunction => aiFunction.AsKernelFunction()));

// Enable automatic function calling
OpenAIPromptExecutionSettings executionSettings = new()
{
    Temperature = 0,
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: new() { RetainArgumentTypes = true })
};

// Test using Jira tools
string? userInput;
Console.WriteLine("Welcome to the Jira Agent! Type 'exit' to quit.");
Console.WriteLine("First mode - Tool Mode");
Console.WriteLine("You can ask me anything about Jira projects, issues, and workflows, and I will use the tools to answer your questions. Example:");
Console.WriteLine("\tShow me the open issues in project ABC");
Console.WriteLine("\tCreate a new issue in project XYZ with title 'Bug fix needed'");
Console.WriteLine("\tList all projects I have access to");

do {
    // Collect user input
    Console.Write("User > ");
    userInput = Console.ReadLine();

    // Add user input
    if (string.IsNullOrEmpty(userInput) || userInput == "exit" || userInput == "quit" || userInput == "q")
    {
        userInput = null;
        break;
    }

    var result = await kernel.InvokePromptAsync(userInput, new(executionSettings)).ConfigureAwait(false);
    Console.WriteLine($"Assistant > {result}");

}while (!string.IsNullOrEmpty(userInput));
Console.WriteLine("\n\nTool Mode finished.\n\n");



// Define the agent
ChatCompletionAgent agent = new ChatCompletionAgent()
{
    Instructions = "Answer questions about Jira projects, issues, workflows, and help manage Jira tasks. You can search for issues, create new issues, update existing ones, and provide information about project status.",
    Name = "JiraAgent",
    Kernel = kernel,
    Arguments = new KernelArguments(executionSettings),
};

// Create a chat history agent thread
AgentThread thread = new ChatHistoryAgentThread();

Console.WriteLine("\n\nNew Lets see this as an agent.\n\n");
Console.WriteLine("Second mode - Agent Mode Enabled");
Console.WriteLine("Ask me anything about Jira projects and issues, example:");
Console.WriteLine("\tShow me all issues assigned to me");
Console.WriteLine("\tCreate a bug report for project ABC");
Console.WriteLine("\tWhat's the status of issue KEY-123?");

do {
    // Collect user input
    Console.Write("User > ");
    userInput = Console.ReadLine();

    // Add user input
    if (string.IsNullOrEmpty(userInput) || userInput == "exit" || userInput == "quit" || userInput == "q")
    {
        userInput = null;
        break;
    }

    // Respond to user input, invoking functions where appropriate.
    ChatMessageContent response = await agent.InvokeAsync(userInput, thread).FirstAsync();
    Console.WriteLine($"\n\nResponse from JiraAgent:\n{response.Content}");
}while (!string.IsNullOrEmpty(userInput));
Console.WriteLine("\n\nAgent Mode finished.\n\n");