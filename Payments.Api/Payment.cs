namespace Payments.Api;

public enum PaymentStatus
{
    Pending,
    Succeeded,
    Failed
}

public class Payment
{
    public Guid Id {get;set;}
    public Guid OrderId{get;set;}
    public decimal Amount {get; set;}
    public PaymentStatus Status{get;set;} = PaymentStatus.Pending;
    public DateTime ProcessedAt{get;set;} = DateTime.UtcNow;
}