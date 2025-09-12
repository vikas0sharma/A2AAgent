# Docker Deployment Guide

This guide explains how to run the A2A News Agent application using Docker and Docker Compose.

## Quick Start

### Prerequisites
- Docker Engine 20.10+
- Docker Compose v2.0+

### Using Docker Compose (Recommended)

1. **Clone the repository and navigate to the project directory:**
   ```bash
   git clone <repository-url>
   cd A2AAgent
   ```

2. **Set up environment variables:**
   ```bash
   cp .env.example .env
   # Edit .env file with your actual API keys
   ```

3. **Build and run the application:**
   ```bash
   docker-compose up -d
   ```

4. **Access the application:**
   - API: http://localhost:8080
   - Swagger UI: http://localhost:8080/swagger (in development mode)

### Using Docker directly

1. **Build the Docker image:**
   ```bash
   docker build -t a2a-agent .
   ```

2. **Run the container:**
   ```bash
   docker run -d \
     --name a2a-agent \
     -p 8080:8080 \
     -e NewsApi__ApiKey=your_news_api_key \
     -e Google__ApiKey=your_google_api_key \
     -e Authentication__ApiKey__Key=your_secret_key \
     a2a-agent
   ```

## Configuration

### Environment Variables

The application can be configured using environment variables. Use double underscores (`__`) to represent nested configuration sections:

```bash
# Authentication
Authentication__Type=ApiKey
Authentication__ApiKey__HeaderName=X-Api-Key
Authentication__ApiKey__Key=your_secret_key

# API Keys
NewsApi__ApiKey=your_news_api_key
Google__ApiKey=your_google_api_key
HuggingFace__ApiKey=your_huggingface_api_key

# OAuth2 (if needed)
Authentication__OAuth2__AuthorizationEndpoint=https://your-auth-provider.com/
Authentication__OAuth2__TokenEndpoint=https://your-auth-provider.com/oauth/token
Authentication__OAuth2__Audience=https://your-api.example.com
```