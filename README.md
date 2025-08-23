# A2A News Agent

A sophisticated Agent-to-Agent (A2A) communication system built with .NET 8 that provides real-time news information through AI-powered interactions. This agent leverages Microsoft Semantic Kernel and integrates with News API to deliver current worldwide news through an intelligent conversational interface.

## ?? Features

- **A2A Protocol Implementation**: Full support for Agent-to-Agent communication protocols
- **Multiple Authentication Methods**: Support for Basic, API Key, OAuth2, and OpenID Connect authentication
- **Semantic Kernel Integration**: AI-powered conversations using Google Gemini, Ollama, or HuggingFace models
- **Real-time News**: Integration with News API for live news headlines and breaking news
- **Function Calling**: Automatic invocation of news retrieval functions through AI
- **Streaming Support**: Real-time streaming responses for better user experience
- **RESTful API**: Standard HTTP endpoints for easy integration
- **Swagger Documentation**: Interactive API documentation

## ??? Architecture

### Core Components

- **A2A Server**: Implements the A2A protocol for agent communication
- **Semantic Kernel**: Provides AI capabilities and function calling
- **News Plugin**: Handles news API integration and data retrieval
- **Authentication Handlers**: Multiple authentication strategies
- **Agent Runtime**: Manages agent execution and task processing

### Technology Stack

- **.NET 8**: Latest .NET framework
- **ASP.NET Core**: Web API framework
- **Microsoft Semantic Kernel**: AI orchestration
- **A2A Protocol**: Agent-to-Agent communication
- **News API**: Real-time news data source
- **RestEase**: HTTP client library
- **Swagger/OpenAPI**: API documentation

## ??? Installation & Setup

### Prerequisites

- .NET 8 SDK
- API keys for:
  - News API (https://newsapi.org/)
  - Google AI (Gemini) or other AI providers
  - OAuth2/OpenID Connect providers (if using those auth methods)

### Configuration

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd A2AAgent
   ```

2. **Configure API Keys**
   
   Update `appsettings.json` or use User Secrets:
   ```bash
   dotnet user-secrets set "NewsApi:ApiKey" "your_news_api_key"
   dotnet user-secrets set "Google:ApiKey" "your_google_api_key"
   ```

3. **Authentication Configuration**
   
   Choose your authentication method in `appsettings.json`:
   ```json
   {
     "Authentication": {
       "Type": "ApiKey", // Options: Basic, ApiKey, OAuth2, OpenIdConnect
       "ApiKey": {
         "HeaderName": "X-Api-Key",
         "Key": "your_secret_key"
       }
     }
   }
   ```

4. **Run the Application**
   ```bash
   dotnet run
   ```

## ?? API Reference

### Endpoints

#### Chat Endpoint
```
POST /api/chat
Content-Type: application/json
Authorization: [Based on configured auth method]

{
  "message": "Get me the latest technology news"
}
```

#### A2A Endpoints
- `GET /.well-known/a2a-agent` - Agent discovery
- `POST /a2a` - A2A protocol communication

### Authentication

The API supports multiple authentication methods:

1. **API Key**: Include `X-Api-Key` header (default)
2. **Basic Auth**: Standard username/password
3. **OAuth2**: JWT Bearer tokens
4. **OpenID Connect**: OIDC tokens

## ?? Configuration Options

### Authentication Types

```json
{
  "Authentication": {
    "Type": "OAuth2",
    "OAuth2": {
      "AuthorizationEndpoint": "https://your-auth-provider.com/",
      "TokenEndpoint": "https://your-auth-provider.com/oauth/token",
      "Audience": "https://your-api.example.com"
    }
  }
}
```

### AI Models

Currently supported AI providers:
- **Google Gemini** (default): `gemini-2.0-flash`
- **Ollama**: Local AI models
- **HuggingFace**: Various models through HF API

### News API Configuration

```json
{
  "NewsApi": {
    "BaseUrl": "https://newsapi.org/v2",
    "ApiKey": "your_news_api_key"
  }
}
```

## ?? Usage Examples

### Getting News Headlines

```bash
curl -X POST "https://localhost:7000/api/chat" \
  -H "Content-Type: application/json" \
  -H "X-Api-Key: your_api_key" \
  -d "\"Get me the latest sports news from the US\""
```

### A2A Communication

```bash
curl -X GET "https://localhost:7000/.well-known/a2a-agent" \
  -H "X-Api-Key: your_api_key"
```

## ?? Available Functions

The agent automatically detects and can call these functions:

### `get_top_headlines`
Retrieves live top and breaking headlines for a country or category.

**Parameters:**
- `country` (string): 2-letter ISO country code (default: "us")
- `category` (string): News category (business, entertainment, general, health, science, sports, technology)
- `query` (string): Keywords to search for
- `pageSize` (int): Number of results (default: 20)
- `page` (int): Page number (default: 1)

## ?? Security Features

- **Multiple Authentication Methods**: Flexible auth options for different use cases
- **Secure Key Storage**: Support for User Secrets and environment variables
- **Time-Constant Comparison**: Protection against timing attacks
- **Authorization Policies**: Role-based access control
- **HTTPS Enforcement**: Secure communication by default

## ?? Development

### Project Structure

```
A2AAgent/
??? Controllers/
?   ??? ChatController.cs          # Main chat API endpoint
??? Services/
?   ??? NewsPlugin.cs              # Semantic Kernel news plugin
?   ??? INewsApi.cs                # News API interface
?   ??? Dtos.cs                    # Data transfer objects
??? Authentication/
?   ??? BasicAuthenticationHandler.cs
?   ??? ApiKeyAuthenticationHandler.cs
??? StartupExtensions.cs           # Service configuration
??? AgentRuntime.cs               # A2A agent runtime
??? Program.cs                    # Application entry point
??? appsettings.json              # Configuration
```

### Building and Testing

```bash
# Build the project
dotnet build

# Run tests (if available)
dotnet test

# Run in development mode
dotnet run --environment Development
```

## ?? Monitoring & Logging

The application includes comprehensive logging:
- Authentication events
- API calls to News API
- Semantic Kernel function executions
- A2A protocol communications

Logs are configured through `appsettings.json` and can be directed to various providers.

## ?? Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## ?? License

This project is licensed under the MIT License - see the LICENSE file for details.

## ?? Support

For issues and questions:
1. Check the existing issues in the repository
2. Create a new issue with detailed information
3. Include logs and configuration (without sensitive data)

## ?? Related Resources

- [A2A Protocol Documentation](https://github.com/microsoft/a2a-net)
- [Microsoft Semantic Kernel](https://github.com/microsoft/semantic-kernel)
- [News API Documentation](https://newsapi.org/docs)
- [.NET 8 Documentation](https://docs.microsoft.com/en-us/dotnet/)

---

**Author**: Vikas Sharma  
**GitHub**: [vikas0sharma](https://github.com/vikas0sharma)