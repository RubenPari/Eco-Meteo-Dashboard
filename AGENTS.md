# Agent Guidelines for Eco-Meteo-Dashboard

## Project Overview
- **Type**: ASP.NET Core Blazor Web App (.NET 9)
- **Framework**: Blazor Server with Interactive Server Components
- **Language**: C# 12+

## Build & Development Commands

### Build
```bash
dotnet build
dotnet build Eco-Meteo-Dashboard.csproj
```

### Run
```bash
dotnet run
dotnet watch run    # Hot reload during development
```

### Publish
```bash
dotnet publish -c Release
```

### Single Test (if tests exist)
```bash
dotnet test --filter "FullyQualifiedName~TestClassName.MethodName"
dotnet test --filter "FullyQualifiedName~Namespace.ClassName.MethodName"
```

### Clean & Rebuild
```bash
dotnet clean
dotnet build --no-incremental
```

## Code Style Guidelines

### General
- Use .NET 10 with `ImplicitUsings` enabled and `Nullable` reference types enabled
- Follow C# naming conventions (PascalCase for types/members, camelCase for locals)
- Keep code concise; avoid unnecessary abstractions

### Imports
- Rely on implicit usings in .csproj (no explicit `using` statements needed for System, collections, etc.)
- Add explicit imports in `.razor` files only when the namespace isn't in `_Imports.razor`
- Organize imports: System libs first, then Microsoft, then project-specific

### Formatting
- Use standard Visual Studio / Rider indentation (4 spaces)
- Enable format on save in your IDE
- Keep line length reasonable (<120 characters when practical)

### Types & Nullability
- Use nullable reference types (`string?`, `T?`) - enabled in project
- Prefer `record` types for immutable data DTOs
- Use `DateOnly` for dates, `TimeOnly` for times (see Weather.razor example)

### Razor Components (.razor)
- Place `@page` directive at the top
- Put `@attribute` directives below `@page`
- Use `<PageTitle>` component for titles
- Put `@code { }` block at the bottom of the file
- Prefix private fields with underscore: `private WeatherForecast[]? _forecasts;`

### Naming Conventions
- **Classes/Records**: `PascalCase` (e.g., `WeatherForecast`)
- **Methods**: `PascalCase` (e.g., `OnInitializedAsync`)
- **Properties**: `PascalCase` (e.g., `TemperatureC`)
- **Private fields**: `_camelCase` (e.g., `_forecasts`)
- **Parameters**: `camelCase`
- **Files**: `PascalCase.razor` or `PascalCase.cs`

### Error Handling
- Use try-catch blocks for operations that may fail
- In Blazor, use `try-catch` in async lifecycle methods
- Leverage built-in error pages (`/Error`, `/not-found`)
- Log errors via `ILogger` injection when available

### Logging
- Inject `ILogger<T>` for component logging
- Use appropriate log levels: `LogInformation`, `LogWarning`, `LogError`

### Configuration
- Use `appsettings.json` for configuration
- Use `appsettings.Development.json` for dev-specific settings
- Access via `IConfiguration` or strongly-typed options pattern

### Testing
- Create test projects using `xUnit`, `NUnit`, or `MSTest`
- Use `dotnet new xunit` to create a test project
- Follow arrange-act-assert pattern
- Mock dependencies with `Moq` or similar

### Common Patterns

#### Dependency Injection in Components
```csharp
@inject IWeatherService WeatherService

@code {
    protected override async Task OnInitializedAsync()
    {
        var data = await WeatherService.GetForecastAsync();
    }
}
```

#### Using Enums
```csharp
public enum WeatherCondition
{
    Sunny,
    Cloudy,
    Rainy
}
```

### VS Code Integration
- Install C# Dev Kit or Microsoft C# extension
- Use `.vscode/tasks.json` for build/watch commands
- Use `.vscode/launch.json` for debugging

### Project Structure
```
Eco-Meteo-Dashboard/
├── Components/
│   ├── Pages/          # Route pages (@page directives)
│   ├── Layout/         # Layout components (MainLayout, NavMenu)
│   ├── App.razor       # Root component
│   ├── Routes.razor    # Router configuration
│   └── _Imports.razor  # Global usings for components
├── Program.cs          # Entry point, service registration
├── *.csproj           # Project file
└── appsettings.json   # Configuration
```

### Key Project Settings (csproj)
- `TargetFramework`: net9.0
- `Nullable`: enable
- `ImplicitUsings`: enable
- `BlazorDisableThrowNavigationException`: true

### Resources
- [Blazor Documentation](https://learn.microsoft.com/aspnet/core/blazor)
- [.NET Documentation](https://learn.microsoft.com/dotnet)
