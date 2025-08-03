using Agent2AgentProtocol.Discovery.Service;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ICapabilityRegistry, InMemoryCapabilityRegistry>();

WebApplication app = builder.Build();

app.MapPost("/register", (AgentCapability capability, AgentEndpoint endpoint, ICapabilityRegistry registry) =>
{
    registry.RegisterCapability(capability, endpoint);
    return Results.Ok();
});

app.MapGet("/resolve/{capability}", (string capability, ICapabilityRegistry registry) =>
{
    AgentEndpoint? endpoint = registry.ResolveCapability(capability);
    return endpoint is not null ? Results.Ok(endpoint) : Results.NotFound();
});

await app.RunAsync();
