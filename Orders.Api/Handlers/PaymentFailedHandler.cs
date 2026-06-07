using Microsoft.EntityFrameworkCore;
using OrderFlow.Contracts;
using Wolverine;

namespace Orders.Api.Handlers;

public static class PaymentFailedHandler
{
    public static async Task Handle(
        PaymentFailed message,
        OrdersDbContext db,
        IMessageBus bus)
    {
        var order = await db.Orders
            .FirstOrDefaultAsync(o => o.Id == message.OrderId);

        if (order is null) return;
        if (order.Status != OrderStatus.Pending) return;

        await bus.PublishAsync(new ReleaseStock(
            order.Id,
            order.ProductId,
            order.Quantity));

        await db.SaveChangesAsync();
    }
}