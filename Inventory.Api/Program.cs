using Microsoft.EntityFrameworkCore;
using Inventory.Api;
using System.Data.Common;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.RabbitMQ;
using Wolverine.Postgresql;

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
        db.StockItems.Add(new StockItem
        {
           Id = Guid.NewGuid(),
           ProductId = "prod-1",
           AvailableQuantity = 100,
           ReservedQuantity = 0
        });
        db.SaveChanges();
    }
}

app.MapGet("/", () => "Inventory.Api is running.");

app.MapGet("/stock", async (InventoryDbContext db) =>
    await db.StockItems.ToListAsync());

app.MapDefaultEndpoints();
app.Run();