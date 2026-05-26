var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var orders = builder.AddProject<Projects.Orders_Api>("orders")
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.OrderFlow_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(orders)
    .WaitFor(orders);

builder.Build().Run();