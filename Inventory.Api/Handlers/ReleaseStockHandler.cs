using Microsoft.EntityFrameworkCore;
using OrderFlow.Contracts;
using Wolverine;

namespace Inventory.Api.Handlers;

public static class StockReservedHandler{
    public static async Task Handle(
        ReleaseStock message,
        InventoryDbContext db,
        IMessageBus bus)
    {
        var stock = await db.StockItems
            .FirstOrDefaultAsync(s => s.ProductId == message.ProductId);

        if (stock is null) return;

        stock.ReservedQuantity -=message.Quantity;
        stock.AvailableQuantity += message.Quantity;

        await bus.PublishAsync(new StockReleased(message.OrderId));

        await db.SaveChangesAsync();
    }
}