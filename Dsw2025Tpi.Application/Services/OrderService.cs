using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;

namespace Dsw2025Tpi.Application.Services
{
    public class OrdersManagementService
    {
        private readonly IRepository _repository;
        public OrdersManagementService(IRepository repository)
        {
            _repository = repository;
        }

        public async Task<OrderModel.Response> addOrder(OrderModel.Request order)
        {
            if (string.IsNullOrWhiteSpace(order.ShippingAddress) || string.IsNullOrWhiteSpace(order.BillingAddress) || order.OrderItems == null)
                throw new ArgumentException("Los datos ingresados de la orden no son válidos.");

            if (await _repository.First<Customer>(p => p.Id == order.CustomerId) == null)
                throw new ArgumentException($"Cliente con el ID {order.CustomerId} no encontrado en la base de datos.");

            OrderModel.OrderItemRequest itemDuplicate, itemNew;
            var itemsOrderFinish = new List<OrderModel.OrderItemRequest>();

            foreach (var q in order.OrderItems.ToList())
            {
                if (!itemsOrderFinish.Any())
                {
                    itemsOrderFinish.Add(q);
                }
                else if (itemsOrderFinish.Exists(p => p.ProductId == q.ProductId))
                {
                    itemDuplicate = itemsOrderFinish.Find(p => p.ProductId == q.ProductId);
                    itemsOrderFinish.Remove(itemDuplicate);

                    var combinedQuantity = itemDuplicate.Quantity + q.Quantity;

                    itemNew = new OrderModel.OrderItemRequest(
                        itemDuplicate.ProductId,
                        itemDuplicate.Name,
                        itemDuplicate.Description,
                        itemDuplicate.CurrentUnitPrice,
                        combinedQuantity
                    );

                    itemsOrderFinish.Add(itemNew);
                }
                else
                {
                    itemsOrderFinish.Add(q);
                }
            }

            decimal TotalAmount = 0;
            List<OrderItem> allItems = new List<OrderItem>();
            List<Product> allProductsUpdate = new List<Product>();
            var allProducts = await _repository.GetAll<Product>();
            var orderAdd = new Order();

            foreach (var q in itemsOrderFinish)
            {
                if (q.Quantity <= 0)
                    throw new ArgumentException("Cantidad de uno de los productos menor/igual a cero.");

                var product = allProducts.FirstOrDefault(p =>
                    p.Id == q.ProductId &&
                    p.Name == q.Name &&
                    p.CurrentUnitPrice == q.CurrentUnitPrice
                );

                if (product == null)
                    throw new EntityNotFoundException($"Producto con ID={q.ProductId}, Nombre={q.Name}, Precio={q.CurrentUnitPrice} no encontrado.");

                if (!product.IsActive)
                    throw new ArgumentException("Existen productos inhabilitados en la orden.");

                if (q.Quantity > product.StockQuantity)
                    throw new ArgumentException("No hay suficiente cantidad de productos para la orden.");

                var itemInOrder = new OrderItem
                {
                    SkuProd = product.Sku,
                    ProductId = product.Id,
                    Quantity = q.Quantity,
                    UnitPrice = q.CurrentUnitPrice,
                    Subtotal = q.CurrentUnitPrice * q.Quantity,
                    OrderId = orderAdd.Id
                };

                TotalAmount += itemInOrder.Subtotal;
                allItems.Add(itemInOrder);

                product.StockQuantity -= q.Quantity;
                allProductsUpdate.Add(product);
            }

            orderAdd.Date = DateTime.Now;
            orderAdd.ShippingAddress = order.ShippingAddress;
            orderAdd.BillingAddress = order.BillingAddress;
            orderAdd.Notes = order.Notes;
            orderAdd.TotalAmount = TotalAmount;
            orderAdd.CustomerId = order.CustomerId;
            orderAdd.OrderItems = allItems;

            foreach (var p in allProductsUpdate)
                await _repository.Update<Product>(p);

            foreach (var o in allItems)
                await _repository.Add(o);

            await _repository.Add(orderAdd);

            return new OrderModel.Response(
                Id: orderAdd.Id,
                Status: orderAdd.Status,
                TotalAmount: orderAdd.TotalAmount,
                Date: orderAdd.Date,
                ShippingAddress: orderAdd.ShippingAddress,
                BillingAddress: orderAdd.BillingAddress,
                Items: allItems.Select(item => new OrderModel.OrderItemResponse(
                    ProductId: allProducts.First(p => p.Sku == item.SkuProd).Id,
                    Name: item.Product?.Name ?? "",
                    Quantity: item.Quantity,
                    UnitPrice: item.UnitPrice,
                    Subtotal: item.Subtotal
                )).ToList()
            );
        }


        // no acoplar la capa del dominio, debo devolver OrderModel.Respons
        public async Task<IEnumerable<OrderModel.Response>> GetAllOrders()
        {
            var orders = await _repository.GetAll<Order>();

            if (orders == null || !orders.Any())
                throw new EntityNotFoundException("No hay órdenes registradas.");

            var orderItems = await _repository.GetAll<OrderItem>();
            var products = await _repository.GetAll<Product>();

            var responses = orders.Select(order =>
            {
                var items = orderItems
                    .Where(i => i.OrderId == order.Id)
                    .Select(item =>
                    {
                        var product = products.FirstOrDefault(p => p.Sku == item.SkuProd);

                        return new OrderModel.OrderItemResponse(
                            ProductId: product?.Id ?? Guid.Empty,
                            Name: product?.Name ?? "",
                            Quantity: item.Quantity,
                            UnitPrice: item.UnitPrice,
                            Subtotal: item.Subtotal
                        );
                    }).ToList();

                return new OrderModel.Response(
                    Id: order.Id,
                    Status: order.Status,
                    TotalAmount: order.TotalAmount,
                    Date: order.Date,
                    ShippingAddress: order.ShippingAddress,
                    BillingAddress: order.BillingAddress,
                    Items: items
                );
            });

            return responses;
        }

        public async Task DeleteOrder(Guid id)
        {
            var orderById = await _repository.GetById<Order>(id);
            if (orderById != null)
            {
                await _repository.Delete(orderById);
            }
            else
            {
                throw new EntityNotFoundException("Orden a inhabilitar No Cargado/Disponible.");
            }
        }

        public async Task<OrderModel.Response> UpdateOrderStatus(Guid id, string newStatus)
        {
            var order = await _repository.GetById<Order>(id);
            if (order == null)
                throw new EntityNotFoundException($"No se encontró una orden con ID {id}");

            if (!Enum.TryParse<OrderStatus>(newStatus, true, out var parsedStatus))
                throw new ArgumentException($"Estado inválido: {newStatus}");

            order.Status = parsedStatus;
            await _repository.Update(order);

            var items = await _repository.Where<OrderItem>(i => i.OrderId == order.Id);
            var products = await _repository.GetAll<Product>();

            return new OrderModel.Response(
                Id: order.Id,
                Status: order.Status,
                TotalAmount: order.TotalAmount,
                Date: order.Date,
                ShippingAddress: order.ShippingAddress,
                BillingAddress: order.BillingAddress,
                Items: items.Select(item => new OrderModel.OrderItemResponse(
                    ProductId: products.First(p => p.Sku == item.SkuProd).Id,
                    Name: products.First(p => p.Sku == item.SkuProd).Name!,
                    Quantity: item.Quantity,
                    UnitPrice: item.UnitPrice,
                    Subtotal: item.Subtotal
                )).ToList()
            );
        }
    }
}