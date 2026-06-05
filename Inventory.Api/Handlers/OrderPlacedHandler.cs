using Microsoft.EntityFrameworkCore;
using OrderFlow.Contracts;
using Wolverine;

namespace Inventory.Api.Handlers;

public static class OrderPlacedHandler
{
    public static async Task Handle(
        OrderPlaced message,
        InventoryDbContext db,
        IMessageBus bus)
    {
        var stock = await db.StockItems
            .FirstOrDefaultAsync(s => s.ProductId == message.ProductId);

        if (stock is null) return;

        stock.AvailableQuantity -= message.Quantity;
        stock.ReservedQuantity  += message.Quantity;

        await bus.PublishAsync(new StockReserved(
            message.OrderId,
            message.ProductId,
            message.Quantity));

        await db.SaveChangesAsync();
    }
}
