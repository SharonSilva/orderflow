using Microsoft.EntityFrameworkCore;
using OrderFlow.Contracts;
using Wolverine;

namespace Orders.Api.Handlers;

public static class StockReservedHandler
{
    public static async Task Handle(
        StockReserved message,
        OrdersDbContext db,
        IMessageBus bus)
    {
        var order = await db.Orders
            .FirstOrDefaultAsync(o => o.Id == message.OrderId);

        if (order is null)  return;
        if (order.Status != OrderStatus.Pending) return;

        await bus.PublishAsync(new PaymentRequested(
            order.Id,
            order.Amount
        ));

        //No SaveChanges needed - we didnt change anything on the order
        //But we still want the publish tobe durablevia the outbox
        //Calling Save changes with no domain changes still triggeres the 
        //Outbox flush, which is what we want
        await db.SaveChangesAsync();
    }
}