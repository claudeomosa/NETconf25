# .NET Native AOT Demo - .NET Conf 2025

A practical demonstration of .NET Native AOT compilation showcasing real-world performance benefits through a simple Quote API built with a **single codebase** that can be compiled in two different modes.

## Demo Overview

This demo compares the same .NET application built in two different modes, highlighting the key benefits of Native AOT:

- **Faster Startup Time**: Applications start in milliseconds, not seconds
- **Smaller Deployment Size**: Single-file executable with minimal dependencies
- **Lower Memory Footprint**: Reduced working set for containerized and serverless scenarios
- **No Runtime Required**: Self-contained executable runs without installing .NET runtime
- **Same Code**: No need to maintain separate codebases - just a different build command

## Project Structure

```
NETconf25/
├── QuoteApi/                   # Single Quote API project
│   ├── Program.cs              # API implementation
│   └── QuoteApi.csproj         # Project file (Native AOT disabled by default)
├── demo-script.sh              # Automated comparison script
├── test-api.sh                 # Quick API testing script
└── README.md                   # This file
```

## The Quote API

A simple API that serves programming quotes:

- `GET /` - API information and available endpoints
- `GET /quote/random` - Random programming quote
- `GET /quote/{id}` - Specific quote by ID
- `GET /quotes/tag/{tag}` - Filter quotes by tag (programming, humor, motivation, etc.)
- `GET /quotes` - All quotes
- `GET /stats` - API statistics including memory usage

## Key Features Demonstrated

### CreateSlimBuilder for Optimized Startup
The project uses `WebApplication.CreateSlimBuilder()` instead of the standard `CreateBuilder()`. This provides:
- Reduced app size by excluding unused features (MVC, Razor Pages, etc.)
- Faster startup time - only includes components needed for Minimal APIs
- Better Native AOT compatibility and smaller binary sizes
- All the essentials: routing, JSON serialization, logging, configuration

### Source Generation for JSON
The project uses `JsonSerializerContext` for AOT-compatible JSON serialization:
```csharp
[JsonSerializable(typeof(Quote))]
[JsonSerializable(typeof(List<Quote>))]
[JsonSerializable(typeof(ApiInfo))]
[JsonSerializable(typeof(StatsResponse))]
internal partial class AppJsonSerializerContext : JsonSerializerContext { }
```

This enables:
- Compile-time JSON serialization code generation
- No reflection required at runtime
- Full Native AOT compatibility
- Faster serialization performance

### Minimal API with AOT Compatibility
The application uses .NET Minimal APIs with async endpoints, fully AOT-compatible.

### Real-time Metrics
The `/stats` endpoint shows current memory usage, perfect for demonstrating Native AOT's memory efficiency.

## Running the Demo

### Quick Start (Automated)

#### Build Standard Version
```bash
cd QuoteApi
dotnet publish -c Release -o ../publish-standard
cd ../publish-standard
./QuoteApi --urls http://localhost:5000
```

#### Build Self-Contained Version
```bash
cd QuoteApi
dotnet publish -c Release -o ../publish-selfcontained --self-contained true
cd ../publish-selfcontained
./QuoteApi --urls http://localhost:5001
```

#### Build Native AOT Version
```bash
cd QuoteApi
dotnet publish -c Release -o ../publish-aot /p:PublishAot=true /p:InvariantGlobalization=true
cd ../publish-aot
./QuoteApi --urls http://localhost:5002
```

Note: You can also uncomment the Native AOT properties in QuoteApi.csproj to enable it by default.

### Testing the API

```bash
# Get API info

# for Standard version
curl http://localhost:5000/
# for Self-Contained version
curl http://localhost:5001/
# for Native AOT version
curl http://localhost:5002/


# Memory and stats 

# for Standard version
curl http://localhost:5000/stats
# for Self-Contained version
curl http://localhost:5001/stats
# for Native AOT version
curl http://localhost:5002/stats
```

## Expected Results

### Build Size Comparison
- **Standard .NET**: ~90-100 MB (includes runtime and libraries)
- **Self-Contained .NET**: ~80-90 MB (includes runtime) 
    * this is expected to be similar to standard as both include the runtime
- **Native AOT**: ~10-15 MB (single optimized executable)

### Startup Time
- **Standard .NET**: ~1000-2000 ms
- **Self-Contained .NET**: ~1000-2000 ms
- **Native AOT**: ~50-200 ms (10x faster)

### Memory Usage
- **Standard .NET**: ~60-80 MB working set
- **Self-Contained .NET**: ~60-80 MB working set
- **Native AOT**: ~20-40 MB working set (50% reduction)

### When to Use Native AOT (1 minute)
"Native AOT is perfect for:
- Microservices and containerized applications
- Serverless functions (AWS Lambda, Azure Functions)
- CLI tools and utilities
- Applications where startup time matters
- Resource-constrained environments

But remember: there are tradeoffs. Dynamic features like reflection are limited. Most modern .NET libraries are AOT-compatible, but always test your dependencies."

## Key Technical Details

### Project Configuration

The project file is set up to support both modes:

**QuoteApi.csproj**:
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <!-- Native AOT Configuration - uncomment to enable by default -->
    <!-- <PublishAot>true</PublishAot> -->
    <!-- <InvariantGlobalization>true</InvariantGlobalization> -->
  </PropertyGroup>
</Project>
```

You can:
1. Leave it commented and use `/p:PublishAot=true` at build time (recommended for demos)
2. Uncomment it to enable Native AOT by default

### AOT Compatibility Features Used

1. **CreateSlimBuilder**: Optimized builder that excludes unused features
2. **Source-Generated JSON Serialization**: Using `JsonSerializerContext` instead of reflection
3. **Minimal APIs**: Fully AOT-compatible routing and endpoint definitions
4. **Static Type Analysis**: All types known at compile time
5. **No Dynamic Code Generation**: No runtime reflection or dynamic assembly loading

## Troubleshooting

### Build Fails on Native AOT
- Ensure you have the latest .NET 10 SDK installed
- On macOS, you may need Xcode command line tools: `xcode-select --install`
- On Linux, install clang and development libraries

### Trimming Warnings
Native AOT uses aggressive trimming. If you see warnings:
- Review the warning messages carefully
- Add `<TrimmerRootAssembly>` for assemblies that should not be trimmed
- Use `[DynamicallyAccessedMembers]` attribute for reflection scenarios

## Additional Resources

- [.NET Native AOT Documentation](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/)
- [ASP.NET Core and Native AOT](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/native-aot)
- [Source Generation for JSON](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/source-generation)


## License

This demo project is provided as educational content for .NET Conf 2025.

---

**Prepared by**: Claude Omosa
**Event**: .NET Conf 2025
**Location**: Microsoft ADC Nairobi, Kenya
**Date**: Novemeber 29th 2025
**Topic**: Native AOT in .NET
