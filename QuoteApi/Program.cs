using System.Diagnostics;
using System.Text.Json.Serialization;

// Start measuring application startup time
var startupTimer = Stopwatch.StartNew();

var builder = WebApplication.CreateSlimBuilder(args);

// Configure JSON serialization to use source-generated serializers.
// This is REQUIRED for Native AOT compatibility because:
// - Native AOT cannot use reflection-based serialization at runtime
// - Source generators create serialization code at compile time
// - This ensures all types are known ahead of time
// - The AppJsonSerializerContext (defined below) tells the compiler which types to generate serializers for
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var app = builder.Build();

// Stop the startup timer and log the result
startupTimer.Stop();
Console.WriteLine($"Application started in {startupTimer.ElapsedMilliseconds}ms");

var quotes = new List<Quote>
{
    new("The only way to do great work is to love what you do.", "Steve Jobs", new[] { "motivation", "work" }),
    new("Innovation distinguishes between a leader and a follower.", "Steve Jobs", new[] { "innovation", "leadership" }),
    new("Code is like humor. When you have to explain it, it's bad.", "Cory House", new[] { "programming", "humor" }),
    new("First, solve the problem. Then, write the code.", "John Johnson", new[] { "programming", "problem-solving" }),
    new("Simplicity is the soul of efficiency.", "Austin Freeman", new[] { "simplicity", "efficiency" }),
    new("Make it work, make it right, make it fast.", "Kent Beck", new[] { "programming", "best-practices" }),
    new("Any fool can write code that a computer can understand. Good programmers write code that humans can understand.", "Martin Fowler", new[] { "programming", "clean-code" }),
    new("Premature optimization is the root of all evil.", "Donald Knuth", new[] { "optimization", "programming" }),
    new("The best error message is the one that never shows up.", "Thomas Fuchs", new[] { "user-experience", "programming" }),
    new("Walking on water and developing software from a specification are easy if both are frozen.", "Edward V. Berard", new[] { "humor", "software-development" })
};

// Root endpoint - Returns API information and available endpoints
// This serves as documentation for API consumers
app.MapGet("/", () =>
{
    return new ApiInfo(
        "Quote API",
        new Endpoints(
            "/quote/random",
            "/quotes/tag/{tag}",
            "/quotes",
            "/stats"
        )
    );
});

// Returns a random quote from the collection
app.MapGet("/quote/random", () =>
{
    var quote = quotes[Random.Shared.Next(quotes.Count)];
    return Results.Ok(quote);
});

// Get quotes filtered by tag endpoint
app.MapGet("/quotes/tag/{tag}", (string tag) =>
{
    var filtered = quotes.Where(q => q.Tags.Contains(tag.ToLower())).ToList();

    if (filtered.Count == 0)
        return Results.NotFound(new ErrorResponse($"No quotes found with tag '{tag}'"));

    return Results.Ok(filtered);
});

// Returns: Complete list of all quotes
app.MapGet("/quotes", () => quotes);

// Returns: Current memory usage (working set)
app.MapGet("/stats", () =>
{
    return new StatsResponse(
        new ProcessInfo(
            $"{Environment.WorkingSet / 1024 / 1024} MB"
        )
    );
});

app.Run();

// Data models using C# record types

record Quote(string Text, string Author, string[] Tags);

record ApiInfo(string Message, Endpoints Endpoints);

record Endpoints(string RandomQuote, string QuotesByTag, string AllQuotes, string Stats);

record ErrorResponse(string Error);

record StatsResponse(ProcessInfo ProcessInfo);

record ProcessInfo(string WorkingSet);

// JSON Source Generator Context - CRITICAL for Native AOT
// This attribute-based approach tells the compiler to generate JSON serialization code at compile time.
// Each [JsonSerializable] attribute registers a type that needs serialization support.
// Why this matters:
// - Standard .NET uses reflection to serialize JSON at runtime (slow, incompatible with Native AOT)
// - Source generators create optimized code at compile time (fast, AOT-compatible)
// - The compiler can see all types and trim unused code
// - Results in faster serialization and smaller binaries
// Note: You must add a [JsonSerializable] attribute for every type that gets serialized to/from JSON
[JsonSerializable(typeof(Quote))]
[JsonSerializable(typeof(List<Quote>))]
[JsonSerializable(typeof(ApiInfo))]
[JsonSerializable(typeof(Endpoints))]
[JsonSerializable(typeof(ErrorResponse))]
[JsonSerializable(typeof(StatsResponse))]
[JsonSerializable(typeof(ProcessInfo))]
[JsonSerializable(typeof(List<string>))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}
