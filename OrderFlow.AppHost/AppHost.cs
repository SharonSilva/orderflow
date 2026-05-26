var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

// A PostgreSQL container, with a named database for Orders.
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin();

var ordersDb = postgres.AddDatabase("orders-db");

var orders = builder.AddProject<Projects.Orders_Api>("orders")
    .WithReference(ordersDb)
    .WaitFor(ordersDb)
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
    .WithReference(orders)
    .WaitFor(orders);

builder.Build().Run();