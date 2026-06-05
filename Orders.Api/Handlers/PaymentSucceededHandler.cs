using Microsoft.EntityFrameworkCore;
using OrderFlow.Contracts;

namespace Orders.Api.Handlers;

public static class PaymentSucceededHandler
{
    public static async Task Handle(
        PaymentSucceeded message,
        OrdersDbContext db)
    {
        var order = await db.Orders
            .FirstOrDefaultAsync(o => o.Id == message.OrderId);

        if (order is null) return;
        if (order.Status != OrderStatus.Pending) return;

        order.Status = OrderStatus.Confirmed;

        await db.SaveChangesAsync();
    }
}