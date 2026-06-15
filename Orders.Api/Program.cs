using Microsoft.EntityFrameworkCore;
using Orders.Api;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.RabbitMQ;
using Wolverine.Postgresql;
using OrderFlow.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowDemoClient", policy =>
    {
        policy.WithOrigins("http://localhost:8000", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.AddNpgsqlDbContext<OrdersDbContext>("orders-db");

builder.Host.UseWolverine(opts =>
{
    var rabbitConn = builder.Configuration.GetConnectionString("rabbit")
        ?? throw new InvalidOperationException(
            "Connection string 'rabbit' is not configured. Set ConnectionStrings__rabbit env var or appsettings.");
    var dbConn = builder.Configuration.GetConnectionString("orders-db")
        ?? throw new InvalidOperationException(
            "Connection string 'orders-db' is not configured. Set ConnectionStrings__orders-db env var or appsettings.");

    opts.UseRabbitMq(new Uri(rabbitConn))
        .AutoProvision()
        .UseConventionalRouting();

    opts.PersistMessagesWithPostgresql(dbConn, "wolverine");
    opts.UseEntityFrameworkCoreTransactions();
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors("AllowDemoClient");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    db.Database.EnsureCreated();
}


app.MapPost("/", async (
    CreateOrderRequest request,
    OrdersDbContext db,
    IMessageBus bus) =>
{
    var order = new Order
    {
        Id = Guid.NewGuid(),
        CustomerId = request.CustomerId,
        ProductId = request.ProductId,
        Quantity = request.Quantity,
        Amount = request.Amount
    };

    db.Orders.Add(order);
    await bus.PublishAsync(new OrderPlaced(
        order.Id,
        order.CustomerId,
        order.ProductId,
        order.Quantity,
        order.Amount   ));
        
    await db.SaveChangesAsync();

    return Results.Created($"/orders/{order.Id}", order.ToDto());
});

app.MapGet("/", async (string? customerId, OrdersDbContext db) =>
{
    var query = db.Orders.AsQueryable();
    if (!string.IsNullOrWhiteSpace(customerId))
    {
        query = query.Where(o => o.CustomerId == customerId);
    }
    var orders = await query
        .OrderByDescending(o=>o.CreatedAt)
        .ToListAsync();
    return orders.Select(o => o.ToDto());
});

app.MapGet("/{id:guid}", async (Guid id, OrdersDbContext db) =>
{
    var order = await db.Orders.FindAsync(id);
    return order is null ? Results.NotFound() : Results.Ok(order.ToDto());
}

);

app.MapHub<OrderHub>("/hubs/orders");

app.MapDefaultEndpoints();

app.Run();

public record CreateOrderRequest(string CustomerId, string ProductId, int Quantity, decimal Amount);

public record OrderDto(
    Guid Id,
    string CustomerId,
    string ProductId,
    int Quantity,
    decimal Amount,
    string Status,
    DateTime CreatedAt
);

internal static class OrderMappings
{
    public static OrderDto ToDto(this Order o) =>
        new(o.Id, o.CustomerId, o.ProductId, o.Quantity, o.Amount, o.Status.ToString(), o.CreatedAt);
}
