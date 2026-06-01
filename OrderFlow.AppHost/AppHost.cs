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
