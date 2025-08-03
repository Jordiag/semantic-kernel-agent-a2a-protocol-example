using Agent2AgentProtocol.Discovery.Service;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ICapabilityRegistry, InMemoryCapabilityRegistry>();

var app = builder.Build();

app.MapPost("/register", (CapabilityRegistration registration, ICapabilityRegistry registry) =>
{
    registry.RegisterCapability(registration.Capability, registration.Endpoint);
    return Results.Ok();
});

app.MapGet("/resolve/{capability}", (string capability, ICapabilityRegistry registry) =>
{
    var endpoint = registry.ResolveCapability(capability);
    return endpoint is not null ? Results.Ok(endpoint) : Results.NotFound();
});

app.Run();
