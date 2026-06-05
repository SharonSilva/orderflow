using System.Data.Common;
using System.Runtime;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Contracts;
using Wolverine;

namespace Payments.Api.Handlers;

public static class PaymentRequestedHandler
{
    public static async Task Handle(
        PaymentRequested message,
        PaymentsDbContext db,
        IMessageBus bus)
    {
        var existing = await db.Payments
            .FirstOrDefaultAsync(p => p.OrderId == message.OrderId);
        if(existing is not null) return;

        var succeeded = message.Amount % 1m != 0m;

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = message.OrderId,
            Amount = message.Amount,
            Status = succeeded ? PaymentStatus.Succeeded : PaymentStatus.Failed,
            ProcessedAt = DateTime.UtcNow
        };

        db.Payments.Add(payment);

        if (succeeded)
        {
            await bus.PublishAsync(new PaymentSucceeded(message.OrderId));
        }
        else
        {
            await bus.PublishAsync(new PaymentFailed(
                message.OrderId,
                "Whole-dollar amounts fall in demo mode"
            ));
        }
        await db.SaveChangesAsync();
    }
}