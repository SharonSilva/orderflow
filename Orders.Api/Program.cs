var builder = WebApplication.CreateBuilder(args);

// Inherit telemetry, health checks, service discovery, resilience.
builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => "Orders.Api is running.");

// Maps /health and /alive from ServiceDefaults.
app.MapDefaultEndpoints();

app.Run();