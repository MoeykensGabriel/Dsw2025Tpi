using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Services
{
    public class OrderService
    {
        private readonly IRepository _repository;

        public OrderService(IRepository repository)
        {
            _repository = repository;
        }

        public async Task<OrderModel.Response> CreateOrder(OrderModel.Request request)
        {
           
            if (request.CustomerId == Guid.Empty ||
                string.IsNullOrWhiteSpace(request.ShippingAddress) ||
                string.IsNullOrWhiteSpace(request.BillingAddress) ||
                request.OrderItems == null || !request.OrderItems.Any())
            {
                throw new ArgumentException("La orden contiene datos inválidos o incompletos.");
            }

            var productIds = request.OrderItems.Select(i => i.ProductId).ToList();

            
            var products = await _repository.GetFiltered<Product>(p => productIds.Contains(p.Id));
            if (products == null || products.Count() != request.OrderItems.Count)
            {
                throw new EntityNotFoundException("Uno o más productos no existen.");
            }

           
            decimal totalAmount = 0;
            var orderItems = new List<OrderItem>();

            foreach (var item in request.OrderItems)
            {
                var product = products.First(p => p.Id == item.ProductId);

                if (product.StockQuantity < item.Quantity)
                {
                    throw new ArgumentException($"Stock insuficiente para el producto '{product.Name}'. Disponible: {product.StockQuantity}, solicitado: {item.Quantity}");
                }

                
                product.StockQuantity -= item.Quantity;

                
                var orderItem = new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = item.CurrentUnitPrice
                };

                totalAmount += orderItem.Subtotal;
                orderItems.Add(orderItem);
            }

            
            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = request.CustomerId,
                ShippingAddress = request.ShippingAddress,
                BillingAddress = request.BillingAddress,
                TotalAmount = totalAmount,
                Date = DateTime.UtcNow,
                Status = OrderStatus.PENDING,
                OrderItems = orderItems
            };

            await _repository.Add(order);
            await _repository.SaveChanges(); 

            
            return new OrderModel.Response(
                order.Id,
                order.Status.ToString(),
                order.TotalAmount,
                order.Date,
                order.ShippingAddress,
                order.BillingAddress,
                order.OrderItems.Select(i => new OrderModel.OrderItemResponse(
                    i.ProductId,
                    i.Name,
                    i.Quantity,
                    i.UnitPrice,
                    i.Subtotal
                )).ToList()
            );
        }
    }
}
