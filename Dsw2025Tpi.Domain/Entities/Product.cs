using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Domain.Entities
{
    public class Product : EntityBase
    {
        public string? Sku { get; set; }
        public string? InternalCode { get;  set; } 
        public string? Name { get; set; }
        public string? Description { get;  set; }
        public decimal CurrentUnitPrice { get;  set; }
        public int StockQuantity { get;  set; }
        public bool IsActive { get;  set; } = true;

        public Product() { }

        public Product(string sku, string name, string description, string internalCode, decimal currentUnitPrice, int stockQuantity) : base()
        {
            this.Sku = sku;
            this.Name = name;
            this.Description = description;
            this.InternalCode = internalCode;
            this.CurrentUnitPrice = currentUnitPrice;
            this.StockQuantity = stockQuantity;
            this.IsActive = true;
        }

        public Product(string sku, string name, string description, string internalCode, decimal currentUnitPrice, int stockQuantity, Guid id) : base(id)
        {
            this.Sku = sku;
            this.Name = name;
            this.Description = description;
            this.InternalCode = internalCode;
            this.CurrentUnitPrice = currentUnitPrice;
            this.StockQuantity = stockQuantity;
            this.IsActive = true;
        }

    }

}
