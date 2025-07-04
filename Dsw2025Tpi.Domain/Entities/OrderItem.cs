using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Domain.Entities
{
    public class OrderItem :  EntityBase
    {
        public Guid OrderId { get; set; }
        public string SkuProd { get; set; }

        public Product? Product { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get ; set; }


        public OrderItem() { }
        public OrderItem(int quantity, decimal unitPrice, Guid orderId, string skuProduct) : base()
        {
            this.Quantity = quantity;
            this.UnitPrice = unitPrice;
            this.SkuProd = skuProduct;
            Subtotal = (quantity * unitPrice);
        }

        public OrderItem(int quantity, decimal unitPrice, string skuProduct) : base()
        {
            this.Quantity = quantity;
            this.UnitPrice = unitPrice;
            this.SkuProd = skuProduct;
            Subtotal = (quantity * unitPrice);
        }
    }
}
