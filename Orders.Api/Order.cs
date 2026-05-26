namespace Orders.Api;

public enum OrderStatus
{
    Pending,
    Confirmed,
    Cancelled
}

public class Order
{
    public Guid Id{get;set;}
    public string CustomerId {get;set;} = default!;
    public string ProductId {get;set;}  = default!;
    public int Quantity {get;set;} 
    public decimal Amount{get;set;}
    public OrderStatus Status {get;set;} = OrderStatus.Pending;
    public DateTime CreatedAt {get;set;} = DateTime.UtcNow;
}