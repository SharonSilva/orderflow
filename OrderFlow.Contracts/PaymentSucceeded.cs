namespace OrderFlow.Contracts;

public record PaymentSucceeded(
    Guid OrderId
);