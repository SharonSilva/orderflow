namespace OrderFlow.Contracts;

public record OrderPlaced(
    Guid OrderId,
    string CustomerId,
    string ProductId,
    int Quantity,
    decimal Amount);
