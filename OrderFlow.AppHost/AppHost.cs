var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");  //it stores data primarily in memory, making it extremely fast
//start a redis container and name it cache
//tie to a variable to connect other services to it 

var postgres = builder.AddPostgres("postgres")  //This adds a postgresSQL server
    .WithDataVolume()   //persist database so it isnt lost when containers restart
    .WithPgAdmin();     //Adds a web interface for postgreSQL


var ordersDb = postgres.AddDatabase("orders-db");
var inventoryDb = postgres.AddDatabase("inventory-db");
var paymentsDb = postgres.AddDatabase("payments-db");

//RabbitMQ message broker, with the management UI plugin enabled
var rabbit = builder.AddRabbitMQ("rabbit")
    .WithManagementPlugin()
    .WithDataVolume();

var orders = builder.AddProject<Projects.Orders_Api>("orders")
    .WithReference(ordersDb)
    .WaitFor(ordersDb)
    .WithReference(rabbit)
    .WaitFor(rabbit)
    .WithHttpHealthCheck("/health");

var inventory = builder.AddProject<Projects.Inventory_Api>("inventory")
    .WithReference(inventoryDb)
    .WaitFor(inventoryDb)
    .WithReference(rabbit)
    .WaitFor(rabbit)
    .WithHttpHealthCheck("/health");

var payments = builder.AddProject<Projects.Payments_Api>("payments")
    .WithReference(paymentsDb)
    .WaitFor(paymentsDb)
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.OrderFlow_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(orders)
    .WaitFor(orders);

builder.Build().Run();
