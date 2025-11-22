using Dsw2025Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Dsw2025Tpi.Data;

public class Dsw2025TpiContext: DbContext
{
    public DbSet<Product> Product {  get; set; }
    public DbSet<Order> Orders { get; set; }    
    public DbSet<OrderItems> OrderItems { get; set; } 

  
        public Dsw2025TpiContext(DbContextOptions<Dsw2025TpiContext> options) : base(options)
    {
    }


    public void LoadData(Dsw2025TpiContext context, string jsonFilePath)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(p =>
        {
            p.Property(x => x.Sku).HasMaxLength(30);
            p.Property(x => x.Name).HasMaxLength(100);
            p.Property(x => x.Description).HasMaxLength(255);
            p.Property(x => x.InternalCode).HasMaxLength(30);
            p.Property(x => x.CurrentUnitPrice).HasColumnType("decimal(18, 2)");
            p.Property(x => x.StockQuantity).HasMaxLength(30).IsRequired();
            p.Property(x => x.ImageUrl).HasMaxLength(1024);
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
            o.Property(x => x.TotalAmount).HasColumnType("decimal(18, 2)").IsRequired();
            o.Property(x => x.Status).IsRequired();
            o.Property(x => x.CustomerId).IsRequired();
            o.Property(x => x.Id).IsRequired().HasColumnName("id");
            o.ToTable("Orders");
            modelBuilder.Entity<Order>().HasKey(x => x.Id);
        });

        modelBuilder.Entity<OrderItems>(t =>
        {
            t.HasKey(x => x.Id);
            t.Property(x => x.SkuProd).HasMaxLength(30);
            t.Property(x => x.Quantity).IsRequired();
            t.Property(x => x.Subtotal).HasColumnType("decimal(18, 2)");
            t.Property(x => x.UnitPrice).HasColumnType("decimal(18, 2)");
            t.Property(x => x.OrderId);
            t.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId); 
            t.Property(x => x.Id).IsRequired().HasColumnName("id");

            t.ToTable("OrderItems");
        });
    }
}
    
   

