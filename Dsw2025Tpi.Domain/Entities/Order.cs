using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Domain.Entities
{
    public class Order
    {

        public Guid CustomerId { get; set; }
        public DateTime Date {  get; set; }
        public string ShipiingAdress { get; set; }
        public string BillingAdress { get; set; }
        public string? Notes { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.PENDING;

        public List<OrderItem> OrderItems { get; set; } = new();
        public decimal TotalAmount => OrderItems.Sum(item => item.Subtotal);
    }
}
