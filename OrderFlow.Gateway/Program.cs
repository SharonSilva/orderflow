var builder = WebApplication.CreateBuilder(args);

// Inherit shared telemetry, health checks, service discovery, resilience.
builder.AddServiceDefaults();

// Register YARP and bind its config from appsettings.json's "ReverseProxy" section.
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

//Enable WebSocket support so YARP can proxy upgrade  through to backends
app.UseWebSockets();

// Standard /health and /alive endpoints from ServiceDefaults.
app.MapDefaultEndpoints();

// Hand all matching requests off to YARP. This is what makes us a gateway.
app.MapReverseProxy();

app.Run();