namespace OrderFlow.Contracts;

public record PaymentRequested(
    Guid OrderId,
    decimal Amount);