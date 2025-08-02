using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Domain.Entities;

public class OrderItems : EntityBase
{
    public Guid OrderId { get; set; }
    public string SkuProd { get; set; }
    public Guid ProductId { get; set; } // esta es la FK q me falto agregar, por eso es NULL la columna
    public Product? Product { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }

    public OrderItems() { }
    public OrderItems(int quantity, decimal unitPrice, Guid orderId, string skuProduct) : base()
    {
        this.Quantity = quantity;
        this.UnitPrice = unitPrice;
        this.SkuProd = skuProduct;
        Subtotal = (quantity * unitPrice);
    }

    public OrderItems(int quantity, decimal unitPrice, string skuProduct) : base()
    {
        this.Quantity = quantity;
        this.UnitPrice = unitPrice;
        this.SkuProd = skuProduct;
        Subtotal = (quantity * unitPrice);
    }
}

