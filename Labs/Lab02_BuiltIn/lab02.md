# 🧰 Lab 02: Working with Built-in Plugins - Google Search Integration

Welcome to the second lab of our Agentic AI Workshop! Building on the foundation established in [Lab 01](../Lab01_LightPluginDemo/lab01.md), we'll now explore how to leverage Semantic Kernel's built-in plugins to enhance your AI agents with external capabilities.

## 🎯 Learning Objectives

- Understand the concept of built-in plugins in Semantic Kernel
- Learn how to integrate the Google Text Search plugin
- Explore different ways to process and present search results
- Build an agent that can search the web and provide informative responses
- Learn how to chain multiple plugins together for complex workflows

## 🔍 Understanding Built-in Plugins

### What are Built-in Plugins?

Built-in or Out-of-The-Box plugins are pre-packaged functionalities that extend what your Semantic Kernel-powered agents can do without requiring you to write custom code from scratch. They provide ready-to-use capabilities such as:

- 🔍 Web searching (Google, Bing)
- 🧮 File System
- 📅 Microsoft Graph
- 🌐 HTTP / Grpc requests
- 📊 Documents

These plugins follow the same architecture as the custom plugins we built in Lab 01 but come pre-implemented with the Semantic Kernel SDK or through additional NuGet packages.

### Benefits of Using Built-in Plugins

- ⏱️ **Save development time**: Use existing functionality instead of creating it yourself
- 🧪 **Tested and optimized**: Built-in plugins are maintained by the Semantic Kernel team
- 🔄 **Consistent interfaces**: Follow the same patterns across plugins
- 🔌 **Easy integration**: Simple to add to your kernel

## 🌐 Google Text Search Plugin Overview

The Google Text Search plugin allows your agent to search the web using Google's Custom Search API. This is extremely valuable for:

- Providing up-to-date information beyond the LLM's training data
- Answering factual queries with current information
- Researching topics from multiple sources
- Creating a Retrieval-Augmented Generation (RAG) system

![Google Search Plugin Diagram](https://learn.microsoft.com/en-us/semantic-kernel/media/concepts_plugins.png)

## 🚀 Getting Started

### Prerequisites

Before starting this lab, ensure you have:

- Completed [Lab 01](../Lab01_LightPluginDemo/lab01.md) to understand Semantic Kernel basics
- .NET SDK 9.0 or later installed
- An IDE like Visual Studio or VS Code
- Access to Azure OpenAI or OpenAI API
- Google API Key and Custom Search Engine ID

### Setting Up Google Custom Search

Before we can use the Google Text Search plugin, we need to set up a Google Custom Search Engine:

1. Create a Google Cloud Platform account if you don't have one
2. Create a new project in the Google Cloud Console
3. Enable the Custom Search API for your project
4. Generate an API key
5. Create a Programmable Search Engine and get your Search Engine ID

Detailed instructions:
1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select an existing one
3. Navigate to "APIs & Services" > "Library"
4. Search for "Custom Search API" and enable it
5. Create credentials to get your API key
6. Go to [Programmable Search Engine](https://programmablesearchengine.google.com/about/)
7. Create a new search engine and note your Search Engine ID

### Project Setup

1. Navigate to the Lab02 directory
2. Create a `.env` file with your API keys:

```
AZURE_OPENAI_ENDPOINT=https://your-endpoint.openai.azure.com
AZURE_OPENAI_KEY=your-api-key
GOOGLE_API_KEY=your-google-api-key
GOOGLE_SEARCH_ENGINE_ID=your-search-engine-id
```

3. Run `dotnet restore` to install dependencies

## 🔧 Step 1: Exploring the Project Structure

The GoogleTextSearch project consists of:

- **Program.cs**: Main application demonstrating Google Text Search functionality
- **GoogleTextSearch.csproj**: Project file with dependencies including the Web plugins package

Key dependencies:
```xml
<PackageReference Include="Microsoft.SemanticKernel" Version="1.48.0" />
<PackageReference Include="Microsoft.SemanticKernel.Plugins.Web" Version="1.48.0-alpha" />
```

Note that this is still in an alpha state, so expect some changes in future releases.

your will need to ignore the following warning about the alpha version:
```
<NoWarn>SKEXP0050</NoWarn>
```


## 🏗️ Step 2: Setting Up the Google Search Plugin

In `Program.cs`, we create and configure the Google Text Search plugin:

```csharp
// Create a text search using Google Text search
var textSearch = new GoogleTextSearch(
    searchEngineId: googleSearchEngineID,
    apiKey: googleAPIKey);

// Build a text search plugin with GoogleSearch search and add to the kernel
var searchPlugin = textSearch.CreateWithSearch("SearchPlugin");
kernel.Plugins.Add(searchPlugin);
```

This code:
1. Creates a `GoogleTextSearch` instance with your Search Engine ID and API key
2. Creates a plugin with search functionality
3. Adds the plugin to the kernel with the name "SearchPlugin"

## 🔄 Step 3: Different Ways to Use Google Search

The Google Text Search plugin offers multiple ways to retrieve and process search results:

### Method 1: Basic String Results

```csharp
var stringResults = await textSearch.SearchAsync(query);
await foreach (string result in stringResults.Results)
{
    Console.WriteLine(result);
}
```

This approach returns simple string results, ideal for quick text-based searches.

### Method 2: Structured Text Search Results

```csharp
KernelSearchResults<TextSearchResult> textResults = 
    await textSearch.GetTextSearchResultsAsync(query, new() { Top = 4, Skip = 4 });

await foreach (TextSearchResult result in textResults.Results)
{
    Console.WriteLine($"Name:  {result.Name}");
    Console.WriteLine($"Value: {result.Value}");
    Console.WriteLine($"Link:  {result.Link}");
}
```

This returns more structured data including name, value, and link for each result.

### Method 3: Full Google Search Results

```csharp
KernelSearchResults<object> fullResults = 
    await textSearch.GetSearchResultsAsync(query, new() { Top = 4, Skip = 8 });

await foreach (Google.Apis.CustomSearchAPI.v1.Data.Result result in fullResults.Results)
{
    Console.WriteLine($"Title:       {result.Title}");
    Console.WriteLine($"Snippet:     {result.Snippet}");
    Console.WriteLine($"Link:        {result.Link}");
    Console.WriteLine($"DisplayLink: {result.DisplayLink}");
}
```

This returns the complete Google search results with all available metadata.

## 💬 Step 4: Creating a Conversational Search Agent

Now let's combine the Google Search plugin with a conversational agent:

```csharp
// Create a history to store the conversation
var history = new ChatHistory();

// Initiate a back-and-forth chat
string? userInput;
do {
    // Get user input
    Console.Write("User > ");
    userInput = Console.ReadLine();

    if (!string.IsNullOrEmpty(userInput))
    {
        history.AddUserMessage(userInput);
        
        // Get the response from the AI, which can use the search plugin
        var result = await chatCompletionService.GetChatMessageContentAsync(
            history,
            executionSettings: openAIPromptExecutionSettings,
            kernel: kernel);
        
        // Print the results
        Console.WriteLine("Assistant > " + result);
        
        // Add the message from the agent to the chat history
        history.AddMessage(result.Role, result.Content ?? string.Empty);
    }
} while (userInput is not null);
```

This creates a conversational agent that can:
1. Take user questions
2. Use the Google Search plugin when needed
3. Incorporate search results into its responses

## 🧪 Hands-on Exercises

### Exercise 1: Run the code and explore search results

Run the provided code to see how the Google Text Search plugin retrieves results. Experiment with different queries and observe how the results vary based on the search parameters.

example: search for "Semantic Kernel"


### Exercise 2: Customize Search Parameters

Modify the search parameters to get different types of results:

```csharp
// Try different search parameters
var customResults = await textSearch.GetTextSearchResultsAsync(
    query, 
    new() { 
        Top = 10,       // Number of results to return
        Skip = 0,       // Number of results to skip
        Count = true,   // Whether to include the total count
        SafeSearch = SafeSearchLevel.Moderate  // Filter explicit content
    }
);
```

### Exercise 2: Create a RAG Agent

Create a Retrieval-Augmented Generation (RAG) agent that:
1. Takes a user question
2. Searches the web for relevant information
3. Processes and summarizes the search results
4. Provides a comprehensive answer

```csharp
// Example implementation of the RAG pattern
async Task<string> GetRAGResponseAsync(string query)
{
    // 1. Retrieve information
    var searchResults = await textSearch.SearchAsync(query);
    var context = new List<string>();
    
    await foreach (var result in searchResults.Results.Take(3))
    {
        context.Add(result);
    }
    
    // 2. Generate a response using the retrieved information
    var contextString = string.Join("\n\n", context);
    var promptTemplate = $"Based on this information:\n{contextString}\n\nAnswer the question: {query}";
    
    var history = new ChatHistory();
    history.AddUserMessage(promptTemplate);
    
    var response = await chatCompletionService.GetChatMessageContentAsync(
        history,
        kernel: kernel);
    
    return response.Content ?? string.Empty;
}
```

### Exercise 3: Multi-Plugin Chain

Create a workflow that chains multiple plugins:

1. Use Google Search to find information
2. Process and extract key points
3. Format the information as a structured report

## 🚀 Challenge: Create a Research Assistant

Build a complete research assistant that:

1. Takes a research topic from the user
2. Breaks it down into specific search queries
3. Searches for each sub-topic
4. Synthesizes the information into a comprehensive report
5. Cites all sources used

## 📚 Additional Resources

- [Semantic Kernel Plugins Documentation](https://learn.microsoft.com/en-us/semantic-kernel/agents/plugins/)
- [Google Custom Search API Documentation](https://developers.google.com/custom-search/v1/overview)
- [Retrieval-Augmented Generation Pattern](https://learn.microsoft.com/en-us/semantic-kernel/patterns/rag)
- [Building Search Applications with Semantic Kernel](https://devblogs.microsoft.com/semantic-kernel/search/)

## ✅ Lab Completion Checklist

- [ ] Set up Google Custom Search Engine and API key
- [ ] Configure the Google Text Search plugin
- [ ] Experiment with different search result formats
- [ ] Create a conversational search agent
- [ ] Complete the hands-on exercises
- [ ] Build the research assistant challenge

## 🏁 Next Steps

After completing this lab, you're ready to move on to [Lab 03: Model Context Protocol (MCP)](../Lab03_MCP/lab03.md), where you'll learn about advanced communication patterns between models and applications.