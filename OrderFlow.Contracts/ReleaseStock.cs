namespace OrderFlow.Contracts;

public record ReleaseStock(
    Guid OrderId,
    string ProductId,
    int Quantity);