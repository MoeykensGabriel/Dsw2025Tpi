using Dsw2025Tpi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Dtos
{
        public record OrderModel
        {
            public record OrderItemRequest(
                Guid ProductId,
                string Name,
                string Description,
                decimal CurrentUnitPrice,
                int Quantity
            );

            public record Request(
                Guid CustomerId,
                string ShippingAddress,
                string BillingAddress,
                List<OrderItemRequest> OrderItems
            );

            public record Response(
                Guid Id,
                OrderStatus Status,
                decimal TotalAmount,
                DateTime Date,
                string ShippingAddress,
                string BillingAddress,
                List<OrderItemResponse> Items
            );

            public record OrderItemResponse(
                Guid ProductId,
                string Name,
                int Quantity,
                decimal UnitPrice,
                decimal Subtotal
            );
        }
    
}
