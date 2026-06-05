using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Payments.Api;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.RabbitMQ;
using Wolverine.Postgresql;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

builder.AddNpgsqlDbContext<PaymentsDbContext>("payments-db");
//Npgsql is the provider that lets ef core communicate with PostgreSQL
//PostgreSQL is the database

builder.Host.UseWolverine(opts =>
{
    var rabbitConn = builder.Configuration.GetConnectionString("rabbit")!;
    var dbConn = builder.Configuration.GetConnectionString("payments-db")!;

    opts.UseRabbitMq(new Uri(rabbitConn))
        .AutoProvision()
        .UseConventionalRouting();

    opts.PersistMessagesWithPostgresql(dbConn, "wolverine");
    opts.UseEntityFrameworkCoreTransactions();

}
);

var app = builder.Build();  //Takes everything configured earlier and turnit into a running web application
app.UseExceptionHandler();  //Adds a safety layer that catches unexpected errors
if (app.Environment.IsDevelopment())        //am i running locally (development mode)
{
    app.MapOpenApi();   // exposes api documentation endpoint

    using var scope = app.Services.CreateScope();   //creates a temporary DI container 
    var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>(); //Give a PaymentDbContext instance
    db.Database.EnsureCreated();    //checks if the database exist if not create it 
}

// Health check / basic endpoint to confirm the service is running
app.MapGet("/", () => "Payments.Api is running.");

//GET endpoint to retrieve all payments from the database
//Uses EF Core to asynchronously fetch all Payment records
app.MapGet("/payments", async (PaymentsDbContext db) => 
    await db.Payments.ToListAsync());

//Registers Aspire/standard default endpoints (like health checks, metrics, etc)
app.MapDefaultEndpoints();

//starts the web application and begins listening for HTTP requests
app.Run();