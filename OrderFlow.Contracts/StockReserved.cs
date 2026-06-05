namespace OrderFlow.Contracts;

public record StockReserved(
    Guid OrderId,
    string ProductId,
    int Quantity
);