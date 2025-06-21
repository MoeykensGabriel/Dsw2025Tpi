using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Domain.Entities
{
    public class Product : EntityBase
    {
        public string? Sku { get; init; }
        public string? InternalCode { get;  set; } 
        public string? Name { get; set; }
        public string? Description { get;  set; }
        public decimal CurrentUnitPrice { get;  set; }
        public int StockQuantity { get;  set; }
        public bool IsActive { get;  set; } = true;

        public Product(string sku, string internalCode, string name, string description, decimal price, int stock)
        {
            if (string.IsNullOrWhiteSpace(sku))
                throw new ArgumentException("El SKU es obligatorio.");

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("El nombre es obligatorio.");

            if (price <= 0)
                throw new ArgumentException("El precio debe ser mayor a 0.");

            if (stock < 0)
                throw new ArgumentException("El stock no puede ser negativo.");


            Sku = sku;
            InternalCode = internalCode;
            Name = name;
            Description = description;
            CurrentUnitPrice = price;
            StockQuantity = stock;
            IsActive = true;
        }
        public Product() { }
        
        
    }

}
