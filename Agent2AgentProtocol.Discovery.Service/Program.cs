using Agent2AgentProtocol.Discovery.Service;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ICapabilityRegistry, InMemoryCapabilityRegistry>();

var app = builder.Build();

app.MapPost("/register", (AgentCapability capability, AgentEndpoint endpoint, ICapabilityRegistry registry) =>
{
    registry.RegisterCapability(capability, endpoint);
    return Results.Ok();
});

app.MapGet("/resolve/{capability}", (string capability, ICapabilityRegistry registry) =>
{
    var endpoint = registry.ResolveCapability(capability);
    return endpoint is not null ? Results.Ok(endpoint) : Results.NotFound();
});

app.Run();
