using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Contracts;
 
namespace Orders.Api.Handlers;

public static class PaymentSucceededHandler
{
    public static async Task Handle(
        PaymentSucceeded message,
        OrdersDbContext db,
        IHubContext<OrderHub> hubContext
    )
    {
        var order = await db.Orders
            .FirstOrDefaultAsync(o => o.Id == message.OrderId);
        if(order is null) return;
        if (order.Status != OrderStatus.Pending) return;

        order.Status = OrderStatus.Confirmed;
        await db.SaveChangesAsync();

        await hubContext.Clients.All.SendAsync("OrderStatusChanged", new
        {
           OrderId = order.Id,
           Status = order.Status.ToString(),
           ChangedAt = DateTime.UtcNow 
        });
    }
}