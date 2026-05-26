using System.Diagnostics;

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var orders = builder.AddProject<Projects.Orders_Api>("orders")
    .WithHttpHealthCheck("/health");

var inventory = builder.AddProject<Projects.Inventory_Api>("inventory")
    .WithHttpHealthCheck("/health");

var payments = builder.AddProject<Projects.Payments_Api>("payments")
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.OrderFlow_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(cache)
    .WaitFor(orders);

builder.Build().Run();