using Agent2AgentProtocol.Discovery.Service;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5000");
builder.Services.AddSingleton<ICapabilityRegistry, InMemoryCapabilityRegistry>();

WebApplication app = builder.Build();

app.MapPost("/register", (CapabilityRegistration registration, ICapabilityRegistry registry) =>
{
    registry.RegisterCapability(registration.Capability, registration.Endpoint);
    Console.WriteLine($"/register enpoint requested, {registration.Capability}/ {registration.Endpoint} registered.");
    return Results.Ok();
});

app.MapGet("/resolve/{capability}", (string capability, ICapabilityRegistry registry) =>
{
    AgentEndpoint? endpoint = registry.ResolveCapability(capability);
    Console.WriteLine($"/resolve/{capability} endpoint requested, 'Address:{endpoint?.Address} / TransportType:{endpoint?.TransportType}' responded.");
    return endpoint is not null ? Results.Ok(endpoint) : Results.NotFound();
});

await app.RunAsync();
