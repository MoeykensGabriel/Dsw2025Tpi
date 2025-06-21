using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Domain.Entities
{
    public class Order:EntityBase
    {
        public Guid CustomerId { get; set; }
        public DateTime Date {  get; set; }
        public string ShipiingAdress { get; set; }
        public string BillingAdress { get; set; }
        public string? Notes { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.PENDING;
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public decimal TotalAmount => OrderItems.Sum(item => item.Subtotal);

        public Order() { }
        public Order(Guid customerId, string shippingAddress, string billingAddress, List<OrderItem> items, string? notes = null)
        {
            CustomerId = customerId;
            ShipiingAdress = shippingAddress;
            BillingAdress = billingAddress;
            Notes = notes;
            OrderItems = items;
            Date = DateTime.UtcNow;
            Status = OrderStatus.PENDING;
        }
    }
}
