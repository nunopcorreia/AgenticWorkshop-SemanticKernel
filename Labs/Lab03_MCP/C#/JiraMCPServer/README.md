# Jira MCP Server Agent

This project demonstrates how to create a Semantic Kernel agent that integrates with Jira using the Model Context Protocol (MCP).

## Setup

### Prerequisites
- .NET 9.0 or later
- Node.js and npm (for the Jira MCP server)
- Azure OpenAI service deployment
- Jira Cloud account with API access

### Configuration

1. **Azure OpenAI Setup**
   - Create an Azure OpenAI resource
   - Deploy a GPT-4o model
   - Copy the endpoint and API key

2. **Jira API Setup**
   - Log into your Jira Cloud instance
   - Go to Account Settings > Security > API tokens
   - Create a new API token
   - Note your email address and Jira server URL

3. **Environment Variables**
   Update the `.env` file with your credentials:
   ```
   AZURE_OPENAI_ENDPOINT="https://your-openai-endpoint.openai.azure.com/"
   AZURE_OPENAI_KEY="your-azure-openai-key"
   JIRA_API_TOKEN="your-jira-api-token"
   JIRA_EMAIL="your-email@company.com"
   JIRA_SERVER_URL="https://your-company.atlassian.net"
   ```

### Running the Application

1. Install dependencies:
   ```bash
   dotnet restore
   ```

2. Run the application:
   ```bash
   dotnet run
   ```

## Usage

The application provides two modes:

### Tool Mode
Direct interaction with Jira tools through the kernel. Example queries:
- "Show me the open issues in project ABC"
- "Create a new issue in project XYZ with title 'Bug fix needed'"
- "List all projects I have access to"

### Agent Mode
Conversational interface with the Jira agent. Example queries:
- "Show me all issues assigned to me"
- "Create a bug report for project ABC"
- "What's the status of issue KEY-123?"

## Features

The Jira MCP Server provides tools for:
- Searching and filtering issues
- Creating new issues
- Updating existing issues
- Managing projects
- Working with workflows
- User and permission management

## Architecture

The application uses:
- **Semantic Kernel**: For AI orchestration and function calling
- **Azure OpenAI**: For language model capabilities
- **Model Context Protocol**: For standardized tool integration
- **Jira MCP Server**: For Jira API integration

The MCP client connects to the Jira server running as a separate Node.js process, providing a clean separation between the AI agent and the Jira integration.
