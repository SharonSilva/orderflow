using Microsoft.EntityFrameworkCore;
using Inventory.Api;
using System.Data.Common;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.RabbitMQ;
using Wolverine.Postgresql;
using RabbitMQ.Client;
using Microsoft.CodeAnalysis.CSharp.Syntax;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

builder.AddNpgsqlDbContext<InventoryDbContext>("inventory-db");

builder.Host.UseWolverine(opts =>
{
    var rabbitConn = builder.Configuration.GetConnectionString("rabbit")!;
    var dbConn = builder.Configuration.GetConnectionString("inventory-db")!;

    opts.UseRabbitMq(new Uri(rabbitConn))
        .AutoProvision()
        .UseConventionalRouting();

    opts.PersistMessagesWithPostgresql(dbConn, "wolverine");
    opts.UseEntityFrameworkCoreTransactions();
});

var app = builder.Build();

app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    using var scope =  app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    db.Database.EnsureCreated();

    if (!db.StockItems.Any())
    {
        db.StockItems.AddRange(
            new StockItem
            {
                Id = Guid.NewGuid(),
                ProductId = "prod-1",
                Name = "Premium Widget",
                UnitPrice = 29.99m,
                AvailableQuantity = 100,
                ReservedQuantity = 0 
            },

            new StockItem
            {
                Id = Guid.NewGuid(),
                ProductId = "prod-2",
                Name = "Deluxe Gadget",
                UnitPrice = 49.99m,
                AvailableQuantity = 50,
                ReservedQuantity = 0
            },
            new StockItem
            {
                Id = Guid.NewGuid(),
                ProductId = "prod-3",
                Name = "Starter Kit",
                UnitPrice = 19.99m,
                AvailableQuantity = 200,
                ReservedQuantity = 0
            }
        );
        db.SaveChanges();
    }
}

app.MapGet("/", () => "Inventory.Api is running.");

app.MapGet("/stock", async (InventoryDbContext db) =>
    await db.StockItems.ToListAsync());

app.MapGet("/products", async (InventoryDbContext db) =>
{
    var items = await db.StockItems
        .OrderBy(s => s.Name)
        .ToListAsync();
    return items.Select(s => s.ToDto());
    
});
app.MapDefaultEndpoints();
app.Run();

public record ProductDto(
    Guid Id,
    string ProductId,
    string Name,
    decimal UnitPrice,
    int AvailableQuantity,
    int ReservedQuantity
);

internal static class ProductMappings
{
    public static ProductDto ToDto(this StockItem s) =>
        new(s.Id, s.ProductId, s.Name, s.UnitPrice, s.AvailableQuantity, s.ReservedQuantity);
}