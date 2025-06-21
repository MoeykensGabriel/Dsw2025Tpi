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
        public string? InternalCode { get; private set; } 
        public string? Name { get; private set; }
        public string? Description { get; private set; }
        public decimal CurrentUnitPrice { get; private set; }
        public int StockQuantity { get; private set; }
        public bool IsActive { get; private set; } = true;

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
