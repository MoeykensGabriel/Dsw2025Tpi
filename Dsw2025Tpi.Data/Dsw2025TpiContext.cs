using Dsw2025Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dsw2025Tpi.Data;

public class Dsw2025TpiContext: DbContext
{
    public DbSet<Product> Product {  get; set; }
    public DbSet<Order> Orders { get; set; }    
    public DbSet<OrderItem> OrderItems { get; set; } 

    public Dsw2025TpiContext(DbContextOptions options): base(options)
    {

    }
    
    // 
}
