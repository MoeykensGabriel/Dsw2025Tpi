using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dsw2025Tpi.Application.Services;

public class OrdersManagementService
{
    private readonly IRepository _repository;
    private readonly ILogger<OrdersManagementService> _logger;
    public OrdersManagementService(IRepository repository, ILogger<OrdersManagementService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<OrderModel.Response> addOrder(OrderModel.Request order)
    {
        if (string.IsNullOrWhiteSpace(order.ShippingAddress) ||
            string.IsNullOrWhiteSpace(order.BillingAddress) ||
            order.OrderItems == null)
            throw new BadRequestException("Los datos ingresados de la orden no son válidos.");
        
        if (await _repository.First<Customer>(p => p.Id == order.CustomerId) == null)
            throw new EntityNotFoundException($"Cliente con el ID {order.CustomerId} no encontrado en la base de datos.");
        
        // verificar duplicados
        var itemsOrderFinish = order.OrderItems
            .GroupBy(i => i.ProductId)
            .Select(g => new OrderModel.OrderItemRequest(g.Key, g.Sum(x => x.Quantity)))
            .ToList();

        decimal TotalAmount = 0;
        List<OrderItems> allItems = new List<OrderItems>();
        List<Product> allProductsUpdate = new List<Product>();
        var allProducts = await _repository.GetAll<Product>();
        var orderAdd = new Order();

        foreach (var q in itemsOrderFinish)
        {
            if (q.Quantity <= 0)
                throw new BadRequestException("Cantidad de uno de los productos menor/igual a cero.");
               
            var product = allProducts.FirstOrDefault(p => p.Id == q.ProductId);

            if (product == null)
                throw new EntityNotFoundException($"Producto con ID={q.ProductId} no encontrado.");
            
            if (!product.IsActive)
                throw new ConflictException("Existen productos inhabilitados en la orden.");
            
            if (q.Quantity > product.StockQuantity)
                throw new ConflictException($"Stock insuficiente para el producto {product.Name}.");
            
            var itemInOrder = new OrderItems
            {
                SkuProd = product.Sku,
                ProductId = product.Id,
                Quantity = q.Quantity,
                UnitPrice = product.CurrentUnitPrice,
                Subtotal = product.CurrentUnitPrice * q.Quantity,
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
                ProductId: item.ProductId,
                Name: allProducts.First(p => p.Id == item.ProductId).Name,
                Quantity: item.Quantity,
                UnitPrice: item.UnitPrice,
                Subtotal: item.Subtotal
            )).ToList()
        );
    }

    // no acoplar la capa del dominio, debo devolver OrderModel.Response
    public async Task<IEnumerable<OrderModel.Response>> GetAllOrders()
    {
        var orders = await _repository.GetAll<Order>();

        if (orders == null || !orders.Any()) 
            throw new EntityNotFoundException("No hay órdenes registradas.");
        
        var orderItems = await _repository.GetAll<OrderItems>();
        var products = await _repository.GetAll<Product>();
        _logger.LogInformation("Se listaron {Count} ordenes", orders.Count());

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

   /* metodo deshabilitado temporalmente
    public async Task DeleteOrder(Guid id)
    {
        var orderById = await _repository.GetById<Order>(id);

        await _repository.Delete(orderById
                                 ?? throw new EntityNotFoundException(
                                 "Orden a inhabilitar No Cargado/Disponible"));
    }
   */
    public async Task<OrderModel.Response> UpdateOrderStatus(Guid id, string newStatus)
    {
        var order = await _repository.GetById<Order>(id);
        if (order == null)
            throw new EntityNotFoundException($"No se encontró una orden con ID {id}");
        
        if (!Enum.TryParse<OrderStatus>(newStatus, true, out var parsedStatus))
            throw new BadRequestException($"Estado invalido: {newStatus}");
        
            
        order.Status = parsedStatus;
        await _repository.Update(order);
        _logger.LogInformation(" Estado de la Order {OrderId} actualizado a {NewStatus}",order.Id,order.Status);

        var items = await _repository.Where<OrderItems>(i => i.OrderId == order.Id);
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
