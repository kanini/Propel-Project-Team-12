# Backend - .NET 8 Web API

ASP.NET Core 8.0 Web API with monolithic architecture.

## Structure

```
Backend.Api/
├── Controllers/           # API endpoints
├── Services/             # Business logic layer
├── Repositories/         # Data access layer
├── Models/               # Entity models
├── Middleware/           # Custom middleware
├── Program.cs            # Application entry point
└── appsettings.json      # Configuration
```

## Architecture

**Monolithic Structure** with three-layer architecture:

- **Controllers**: Handle HTTP requests/responses
- **Services**: Contain business logic
- **Repositories**: Manage data access

## Getting Started

### Restore Dependencies

```bash
cd Backend.Api
dotnet restore
```

### Run the Application

```bash
dotnet run
```

Runs on:
- HTTP: [http://localhost:5000](http://localhost:5000)
- HTTPS: [https://localhost:5001](https://localhost:5001)
- Swagger: [http://localhost:5000/swagger](http://localhost:5000/swagger)

### Build for Production

```bash
dotnet build -c Release
```

## Features

- .NET 8 Web API
- Swagger/OpenAPI documentation
- Global exception handling middleware
- CORS configuration for frontend
- Repository pattern ready
- Service layer pattern ready
- Entity Framework Core ready
- Health check endpoint
