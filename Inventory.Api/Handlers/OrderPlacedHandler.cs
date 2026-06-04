using Microsoft.EntityFrameworkCore;
using OrderFlow.Contracts;

namespace Inventory.Api.Handlers;

public static class OrderPlacedHandler
{
    public static async Task Handle(OrderPlaced message, InventoryDbContext db)
    {
        var stock = await db.StockItems
            .FirstOrDefaultAsync(s => s.ProductId == message.ProductId);

        if (stock is null) return;

        stock.AvailableQuantity -= message.Quantity;
        stock.ReservedQuantity  += message.Quantity;

        await db.SaveChangesAsync();
    }
}