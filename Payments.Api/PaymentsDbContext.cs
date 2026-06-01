using System.Dynamic;
using Microsoft.EntityFrameworkCore;
using Payments.Api;

namespace Payments.Api;

//Database context for the Payments Service
//Acts as the bridge between the application and the PostgreSQL database
public class PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : DbContext(options)
{
    //Represents the Payments table in this database
    //EF core uses this DbSet to query, insert, update, and delete payment records
    public DbSet<Payment> Payments => Set<Payment>();
}