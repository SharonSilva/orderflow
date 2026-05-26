using Microsoft.EntityFrameworkCore;
using Orders.Api;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

builder.AddNpgsqlDbContext<OrdersDbContext>("orders-db");

var app = builder.Build();

app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    db.Database.EnsureCreated();
}

app.MapGet("/", () => "Orders.Api is running.");

app.MapPost("/orders", async (CreateOrderRequest request, OrdersDbContext db) =>
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
    await db.SaveChangesAsync();

    return Results.Created($"/orders/{order.Id}", order);
});

app.MapGet("/orders", async (OrdersDbContext db) =>
    await db.Orders.ToListAsync());

app.MapDefaultEndpoints();

app.Run();

public record CreateOrderRequest(string CustomerId, string ProductId, int Quantity, decimal Amount);