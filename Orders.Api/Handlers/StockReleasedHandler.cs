using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Contracts;
using Orders.Api;

namespace Orders.Api.Handlers;

public static class StockReleasedHandler
{
    public static async Task Handle(
        StockReleased message,
        OrdersDbContext db,
        IHubContext<OrderHub> hubContext)
    {
        var order = await db.Orders
            .FirstOrDefaultAsync(o => o.Id == message.OrderId);

        if (order is null) return;
        if (order.Status != OrderStatus.Pending) return;

        order.Status = OrderStatus.Cancelled;
        await db.SaveChangesAsync();

        await hubContext.Clients.All.SendAsync("OrderStatusChanged", new
        {
            OrderId = order.Id,
            Status = order.Status.ToString(),
            ChangedAt = DateTime.UtcNow
        });
    }
}
