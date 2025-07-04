using Dsw2025Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Dsw2025Tpi.Data;

public class Dsw2025TpiContext: DbContext
{
    public DbSet<Product> Product {  get; set; }
    public DbSet<Order> Orders { get; set; }    
    public DbSet<OrderItem> OrderItems { get; set; } 

  
        public Dsw2025TpiContext(DbContextOptions<Dsw2025TpiContext> options) : base(options)
    {
    }

    public void LoadData(Dsw2025TpiContext context)
    {
        context.Database.ExecuteSqlRaw("TRUNCATE TABLE Customers");

        var fileName = @"C:\Users\gabom\Desktop\Dsw2025Tpi\Dsw2025Tpi.Data\Sources\customers.json";
        var read = File.ReadAllText(fileName);
        var data = JsonSerializer.Deserialize<List<Customer>>(read);
        foreach (var c in data)
        {
            context.Add(c);
        }
        context.SaveChanges();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(p =>
        {
            p.Property(x => x.Sku).HasMaxLength(30);
            p.Property(x => x.Name).HasMaxLength(50);
            p.Property(x => x.Description).HasMaxLength(80);
            p.Property(x => x.InternalCode).HasMaxLength(30);
            p.Property(x => x.CurrentUnitPrice).HasMaxLength(30);
            p.Property(x => x.StockQuantity).HasMaxLength(30).IsRequired();
            p.Property(x => x.Id).IsRequired().HasColumnName("id");
            p.ToTable("Products");
            modelBuilder.Entity<Product>().HasKey(x => x.Id);
        });

        modelBuilder.Entity<Order>().Ignore(x => x.OrderItems);

        modelBuilder.Entity<Order>(o =>
        {
            o.Property(x => x.Date).HasMaxLength(30);
            o.Property(x => x.ShippingAddress).HasMaxLength(30);
            o.Property(x => x.Notes).HasMaxLength(30);
            o.Property(x => x.TotalAmount).HasMaxLength(30).IsRequired();
            o.Property(x => x.Status).HasMaxLength(10);
            o.Property(x => x.CustomerId).IsRequired();
            o.Property(x => x.Id).IsRequired().HasColumnName("id");
            o.ToTable("Orders");
            modelBuilder.Entity<Order>().HasKey(x => x.Id);
        });

        modelBuilder.Entity<OrderItem>(t =>
        {
            t.HasKey(x => x.Id);
            t.Property(x => x.SkuProd).HasMaxLength(30);
            t.Property(x => x.Quantity).IsRequired();
            t.Property(x => x.Subtotal);
            t.Property(x => x.OrderId);
            t.Property(x => x.Id).IsRequired().HasColumnName("id");

            t.ToTable("OrderItems");
        });

        modelBuilder.Entity<Customer>(c =>
        {
            c.Property(x => x.Email).HasMaxLength(30);
            c.Property(x => x.Name).HasMaxLength(30);
            c.Property(x => x.PhoneNumber).HasMaxLength(20);
            c.Property(x => x.Id).IsRequired().HasColumnName("id");
            c.ToTable("Customers");
            modelBuilder.Entity<Customer>().HasKey(x => x.Id);
        });
    }
}
    
   

