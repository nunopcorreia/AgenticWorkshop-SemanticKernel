# 🎭 Lab 04: Multi-Agent Orchestration - Group Chat and Collaboration

Welcome to the final lab of our Agentic AI Workshop! In this lab, we'll explore the most advanced concepts in agentic AI: orchestrating multiple agents to collaborate, debate, and solve complex problems together through group chat interactions.

## 🎯 Learning Objectives

- Understand the architecture of multi-agent systems in Semantic Kernel
- Learn how to create and configure multiple specialized agents
- Implement group chat orchestration with AgentGroupChat
- Create custom termination strategies for conversations
- Build agents that can collaborate, debate, and iterate on solutions
- Understand the concepts of agent roles, instructions, and interaction patterns

## 🧠 Multi-Agent System Concepts

### What is Multi-Agent Orchestration?

Multi-agent orchestration is the coordination of multiple AI agents to work together on complex tasks. Instead of having a single agent handle everything, we create specialized agents that:

1. 🎯 **Have specific roles and expertise**: Each agent is designed for particular tasks
2. 💬 **Communicate through structured interactions**: Agents exchange information in organized ways
3. 🔄 **Iterate and improve**: Agents can refine their outputs based on feedback from other agents
4. 🤝 **Collaborate towards common goals**: Multiple agents work together to achieve objectives

### Benefits of Multi-Agent Systems

- **Specialization**: Each agent can be optimized for specific tasks
- **Quality Control**: Agents can review and improve each other's work
- **Scalability**: Complex workflows can be broken down into manageable parts
- **Resilience**: If one agent fails, others can continue working
- **Diverse Perspectives**: Different agents can approach problems from different angles

### Agent Group Chat Architecture

![Multi-Agent Architecture](https://learn.microsoft.com/en-us/semantic-kernel/media/agents-group-chat.png)

The Semantic Kernel Agent Group Chat system includes:

1. **ChatCompletionAgent**: Individual agents with specific roles and instructions
2. **AgentGroupChat**: The orchestration mechanism that manages agent interactions
3. **TerminationStrategy**: Logic to determine when conversations should end
4. **ExecutionSettings**: Configuration for how the group chat operates

## 🚀 Getting Started

### Prerequisites

Before starting this lab, make sure you have:

- Completed Labs 01-03 to understand Semantic Kernel fundamentals
- .NET SDK 9.0 or later installed
- An IDE like Visual Studio or VS Code
- Access to Azure OpenAI or OpenAI API

### Project Overview

The `MultiAgentChat` project demonstrates a creative collaboration scenario where:
- **ArtDirector**: Reviews and approves creative copy based on industry standards
- **CopyWriter**: Creates and refines advertising copy with creativity and brevity

This simulates a real-world creative process where multiple professionals collaborate to produce high-quality content.

### Setting Up the Environment

1. Navigate to the Lab04 directory
2. Create a `.env` file with your API keys:

```
AZURE_OPENAI_ENDPOINT=https://your-endpoint.openai.azure.com
AZURE_OPENAI_KEY=your-api-key
```

3. Run `dotnet restore` to install dependencies

## 🏗️ Step 1: Creating Individual Agents

### Defining Agent Roles and Instructions

The first step in multi-agent orchestration is creating specialized agents with clear roles:

```csharp
string ReviewerName = "ArtDirector";
string ReviewerInstructions =
    """
    You are an art director who has opinions about copywriting born of a love for David Ogilvy.
    The goal is to determine if the given copy is acceptable to print.
    If so, state that it is approved.
    If not, provide insight on how to refine suggested copy without example.
    """;

string CopyWriterName = "CopyWriter";
string CopyWriterInstructions =
    """
    You are a copywriter with ten years of experience and are known for brevity and a dry humor.
    The goal is to refine and decide on the single best copy as an expert in the field.
    Only provide a single proposal per response.
    You're laser focused on the goal at hand.
    Don't waste time with chit chat.
    Consider suggestions when refining an idea.
    """;
```

Key principles for agent instructions:
- **Be specific about the role**: Define exactly what the agent is supposed to do
- **Set clear boundaries**: Specify what the agent should and shouldn't do
- **Include personality traits**: This helps create more natural interactions
- **Define output expectations**: Be clear about the format and content expected

### Creating Agent Instances

Next, we create the actual agent instances with their own kernels:

```csharp
// Create separate kernels for each agent
Kernel reviewerAgentKernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(modelId, endpoint, apiKey)
    .Build();

Kernel writerAgentKernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(modelId, endpoint, apiKey)
    .Build();

// Create the agents
var reviewerAgent = new ChatCompletionAgent() {
    Name = ReviewerName,
    Instructions = ReviewerInstructions,
    Kernel = reviewerAgentKernel,
};

var writerAgent = new ChatCompletionAgent() {
    Name = CopyWriterName,
    Instructions = CopyWriterInstructions,
    Kernel = writerAgentKernel,
};
```

Each agent has:
- **Its own kernel**: This allows for different configurations if needed
- **A unique name**: Used for identification in conversations
- **Specific instructions**: Define the agent's role and behavior

## 🎮 Step 2: Creating the Agent Group Chat

### Setting Up the Group Chat

The `AgentGroupChat` orchestrates the interaction between multiple agents:

```csharp
// Create the Agent Group Chat
AgentGroupChat chat = new (reviewerAgent, writerAgent)
{
    ExecutionSettings =
        new()
        {
            // Here a TerminationStrategy subclass is used that will terminate when
            // an assistant message contains the term "approve".
            TerminationStrategy =
                new ApprovalTerminationStrategy()
                {
                    // Only the art-director may approve.
                    Agents = [reviewerAgent],
                    // Limit total number of turns
                    MaximumIterations = 10,
                }
        }
};
```

The group chat includes:
- **Multiple agents**: The agents that will participate in the conversation
- **Execution settings**: Configuration for how the conversation operates
- **Termination strategy**: Logic to determine when the conversation should end

### Understanding Termination Strategies

Termination strategies are crucial for preventing infinite loops and ensuring conversations conclude appropriately:

```csharp
internal sealed class ApprovalTerminationStrategy : TerminationStrategy
{
    // Terminate when the final message contains the term "approve"
    protected override Task<bool> ShouldAgentTerminateAsync(Agent agent, IReadOnlyList<ChatMessageContent> history, CancellationToken cancellationToken)
        => Task.FromResult(history[history.Count - 1].Content?.Contains("approve", StringComparison.OrdinalIgnoreCase) ?? false);
}
```

This strategy:
- **Checks for specific keywords**: Looks for "approve" in the conversation
- **Limits which agents can terminate**: Only certain agents can end the conversation
- **Prevents infinite loops**: Sets a maximum number of iterations

## 💬 Step 3: Running the Group Chat

### Initiating the Conversation

The group chat runs in a loop, processing user input and agent responses:

```csharp
string? userInput;
do {
    Console.Write("User > ");
    userInput = Console.ReadLine();

    if (string.IsNullOrEmpty(userInput))
    {
        break;
    }

    ChatMessageContent userInputContent = new(AuthorRole.User, userInput);
    chat.AddChatMessage(userInputContent);

    AgentHelper.WriteAgentChatMessage(userInputContent);

    await foreach (ChatMessageContent response in chat.InvokeAsync())
    {
        AgentHelper.WriteAgentChatMessage(response);
    }

    Console.WriteLine($"\n[IS COMPLETED: {chat.IsComplete}]");
} while (chat.IsComplete == false);
```

This loop:
1. **Gets user input**: Collects the initial prompt or feedback
2. **Adds to chat**: Incorporates user input into the conversation
3. **Processes responses**: Agents take turns responding
4. **Displays output**: Shows the conversation as it progresses
5. **Checks completion**: Determines if the conversation should continue

### Understanding Agent Interactions

When the group chat runs, the agents:
1. **Take turns**: Each agent responds in sequence
2. **Build on previous responses**: Agents consider what others have said
3. **Follow their instructions**: Each agent stays in character
4. **Work towards the goal**: The conversation progresses towards completion

## 🔍 Step 4: Advanced Agent Helper

### Displaying Agent Messages

The `AgentHelper` class provides sophisticated message display:

```csharp
public static void WriteAgentChatMessage(ChatMessageContent message)
{
    // Include ChatMessageContent.AuthorName in output, if present.
    string authorExpression = message.Role == AuthorRole.User ? string.Empty : $" - {message.AuthorName ?? "*"}";
    // Include TextContent (via ChatMessageContent.Content), if present.
    string contentExpression = string.IsNullOrWhiteSpace(message.Content) ? string.Empty : message.Content;
    bool isCode = message.Metadata?.ContainsKey(OpenAIAssistantAgent.CodeInterpreterMetadataKey) ?? false;
    string codeMarker = isCode ? "\n  [CODE]\n" : " ";
    Console.WriteLine($"\n# {message.Role}{authorExpression}:{codeMarker}{contentExpression}");

    // Display additional content types
    foreach (KernelContent item in message.Items)
    {
        // Handle different content types (annotations, file references, images, etc.)
        // ...
    }
}
```

This helper:
- **Formats messages clearly**: Shows who is speaking and what they're saying
- **Handles multiple content types**: Can display text, code, images, and more
- **Provides conversation context**: Makes it easy to follow the conversation flow

## 🧪 Hands-on Exercises

### Exercise 1: Create a Code Review Team

Create a multi-agent system for code review:

1. **Developer Agent**: Writes code based on requirements
2. **Security Reviewer**: Checks for security vulnerabilities
3. **Performance Reviewer**: Evaluates code efficiency
4. **Architect**: Ensures code follows design patterns

```csharp
// Example agent configuration
var developerAgent = new ChatCompletionAgent()
{
    Name = "Developer",
    Instructions = """
        You are a senior software developer with expertise in C#.
        Write clean, efficient, and well-documented code.
        Follow SOLID principles and best practices.
        """,
    Kernel = developerKernel,
};

var securityReviewerAgent = new ChatCompletionAgent()
{
    Name = "SecurityReviewer",
    Instructions = """
        You are a security expert who reviews code for vulnerabilities.
        Look for common security issues like SQL injection, XSS, authentication flaws.
        Provide specific recommendations for fixing security issues.
        """,
    Kernel = securityKernel,
};

// Create custom termination strategy
internal sealed class CodeApprovalTerminationStrategy : TerminationStrategy
{
    protected override Task<bool> ShouldAgentTerminateAsync(Agent agent, IReadOnlyList<ChatMessageContent> history, CancellationToken cancellationToken)
    {
        // Terminate when all reviewers have approved
        var lastMessage = history[history.Count - 1].Content ?? "";
        return Task.FromResult(
            lastMessage.Contains("APPROVED", StringComparison.OrdinalIgnoreCase) &&
            agent.Name == "Architect" // Only architect can give final approval
        );
    }
}
```

### Exercise 2: Build a Research Team

Create agents that collaborate on research tasks:

1. **Research Agent**: Gathers information from various sources
2. **Analysis Agent**: Analyzes and synthesizes research findings
3. **Writer Agent**: Creates well-structured reports
4. **Editor Agent**: Reviews and polishes the final output

### Exercise 3: Create a Product Development Team

Build a team that can take a product idea from concept to specification:

1. **Product Manager**: Defines requirements and priorities
2. **Designer**: Creates user experience specifications
3. **Engineer**: Evaluates technical feasibility
4. **QA Agent**: Defines testing strategies

## 🚀 Challenge: Build an AI Software Development Team

Create a comprehensive software development team that can:

1. **Take a feature request**: Process natural language requirements
2. **Create technical specifications**: Break down requirements into technical specs
3. **Design the architecture**: Plan the system design
4. **Write the code**: Implement the solution
5. **Test the implementation**: Create and run tests
6. **Deploy the solution**: Prepare for deployment

Include agents such as:
- **Product Owner**: Clarifies requirements and acceptance criteria
- **System Architect**: Designs the overall system architecture
- **Frontend Developer**: Creates user interfaces
- **Backend Developer**: Implements server-side logic
- **DevOps Engineer**: Handles deployment and infrastructure
- **QA Engineer**: Creates comprehensive test plans

## 📚 Additional Resources

- [Semantic Kernel Agents Documentation](https://learn.microsoft.com/en-us/semantic-kernel/agents/)
- [Multi-Agent Systems Patterns](https://learn.microsoft.com/en-us/semantic-kernel/agents/multi-agent)
- [Agent Group Chat Documentation](https://learn.microsoft.com/en-us/semantic-kernel/agents/group-chat)
- [Building Collaborative AI Systems](https://learn.microsoft.com/en-us/semantic-kernel/agents/collaboration)

## ✅ Lab Completion Checklist

- [ ] Understand multi-agent system architecture
- [ ] Create specialized agents with clear roles
- [ ] Implement agent group chat orchestration
- [ ] Create custom termination strategies
- [ ] Build agents that collaborate effectively
- [ ] Complete the hands-on exercises
- [ ] Build the AI software development team challenge

## 🎉 Workshop Conclusion

Congratulations! You've completed all four labs of the Agentic AI Workshop with Semantic Kernel. You've learned:

1. **Lab 01**: Semantic Kernel fundamentals and custom plugin creation
2. **Lab 02**: Working with built-in plugins and external integrations
3. **Lab 03**: Model Context Protocol and advanced agent architectures
4. **Lab 04**: Multi-agent orchestration and collaborative systems

You're now equipped to build sophisticated agentic AI applications that can:
- Integrate with external systems and APIs
- Collaborate on complex tasks
- Adapt to different scenarios and requirements
- Scale to handle enterprise-level workloads

## 🚀 Next Steps

Now that you've mastered the fundamentals, consider:

- **Building production applications**: Apply these concepts to real-world scenarios
- **Exploring advanced patterns**: Investigate hierarchical agent structures and more complex orchestration
- **Contributing to the community**: Share your experiences and learnings with the Semantic Kernel community
- **Staying updated**: Follow the latest developments in agentic AI and Semantic Kernel

The future of AI is agentic, and you're now ready to be part of building it! 🌟