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
            CustomerId: orderAdd.CustomerId,
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
    public async Task<IEnumerable<OrderModel.Response>> GetAllOrders(
        int pageNumber = 1, int pageSize = 8, string? status = null, string? search = null)
    {
        var orders = await _repository.GetAll<Order>();

        if (orders == null || !orders.Any()) 
            throw new EntityNotFoundException("No hay órdenes registradas.");
        if (!string.IsNullOrEmpty(status))
        {
            // Intentamos convertir el string "PENDING" en el Enum OrderStatus.PENDING
            if (Enum.TryParse<OrderStatus>(status, true, out var parsedStatus))
            {
                // Filtramos en memoria usando LINQ
                orders = orders.Where(o => o.Status == parsedStatus);
            }
            else
            {
                // Si el estado no es válido (ej. "cualquiercosa"), devolvemos una lista vacía
                // o podríamos lanzar un BadRequestException
                return new List<OrderModel.Response>();
            }
        }

        // 2. Aplicar filtro de Búsqueda (si se provee)
        if (!string.IsNullOrEmpty(search))
        {
            var searchTerm = search.ToLowerInvariant().Trim();
            orders = orders.Where(o =>
                // Buscamos si el ID de la orden (como string) contiene el término
                o.Id.ToString().ToLowerInvariant().Contains(searchTerm) ||
                // Buscamos si el ID del cliente (como string) contiene el término
                o.CustomerId.ToString().ToLowerInvariant().Contains(searchTerm)
            );
        }

        // ordenar de mas actual a mas antigua
        orders = orders.OrderByDescending(o => o.Date);

        var skip = (pageNumber - 1) * pageSize; // algoritmo para tomar la cant de orders
        var ordersPag = orders.Skip(skip).Take(pageSize); // ordersPag = lista ya PAGINADA

        _logger.LogInformation("Se listaron {Count} ordenes" +
            " en pagina {pNumber} con tamaño de pagina {pSize}",ordersPag.Count(),pageNumber, pageSize );

        var orderItems = await _repository.GetAll<OrderItems>();
        var products = await _repository.GetAll<Product>();

        var responses = ordersPag.Select(order =>
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
                CustomerId: order.CustomerId,
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

    public async Task<OrderModel.OrdersSummaryResponse> GetOrdersSummary()
    {
        var orders = await _repository.GetAll<Order>();

        if( orders == null  || !orders.Any())
        {
            return new OrderModel.OrdersSummaryResponse(
                TotalOrders: 0,
                PendingOrders: 0,
                ProcessingOrders: 0,
                ShippedOrders: 0,
                DeliveredOrders: 0,
                CancelledOrders: 0
                );
            
        }

        var totalOrders = orders.Count();
        var totalPendingOrders = orders.Count(o => o.Status == OrderStatus.PENDING);
        var totalProcessingOrders = orders.Count(o => o.Status == OrderStatus.PROCESSING);
        var totalDeliveredOrders = orders.Count(o => o.Status == OrderStatus.DELIVERED);
        var totalShippedOrders = orders.Count(o => o.Status == OrderStatus.SHIPPED);
        var totalCancelledOrders = orders.Count(o => o.Status == OrderStatus.CANCELLED);

        return new OrderModel.OrdersSummaryResponse(
                TotalOrders: totalOrders,
                PendingOrders: totalPendingOrders,
                ProcessingOrders: totalProcessingOrders,
                ShippedOrders: totalShippedOrders,
                DeliveredOrders: totalDeliveredOrders,
                CancelledOrders: totalCancelledOrders
                );
    }

    public async Task<OrderModel.Response> GetOrderById(Guid id)
    {
        var order = await _repository.GetById<Order>(id);
        if (order == null)
            throw new EntityNotFoundException($"No se encontró una orden con ID {id}");

        // Reutilizamos la lógica que ya teníamos para buscar items y productos
        var items = await _repository.Where<OrderItems>(i => i.OrderId == order.Id);
        var products = await _repository.GetAll<Product>();

        return new OrderModel.Response(
            Id: order.Id,
            CustomerId: order.CustomerId,
            Status: order.Status,
            TotalAmount: order.TotalAmount,
            Date: order.Date,
            ShippingAddress: order.ShippingAddress,
            BillingAddress: order.BillingAddress,
            Items: items.Select(item => new OrderModel.OrderItemResponse(
                // Usamos FirstOrDefault para ser más seguros
                ProductId: products.FirstOrDefault(p => p.Sku == item.SkuProd)?.Id ?? Guid.Empty,
                Name: products.FirstOrDefault(p => p.Sku == item.SkuProd)?.Name ?? "Producto no encontrado",
                Quantity: item.Quantity,
                UnitPrice: item.UnitPrice,
                Subtotal: item.Subtotal
            )).ToList()
        );
    }

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
            CustomerId: order.CustomerId,
            Status: order.Status,
            TotalAmount: order.TotalAmount,
            Date: order.Date,
            ShippingAddress: order.ShippingAddress,
            BillingAddress: order.BillingAddress,
            Items: items.Select(item => new OrderModel.OrderItemResponse(
                ProductId: products.FirstOrDefault(p => p.Sku == item.SkuProd)?.Id ?? Guid.Empty,
                Name: products.First(p => p.Sku == item.SkuProd).Name! ?? "Producto no encontrado",
                Quantity: item.Quantity,
                UnitPrice: item.UnitPrice,
                Subtotal: item.Subtotal
            )).ToList()
        );
    }
    public async Task DeleteOrder(Guid id)
    {
        var order = await _repository.GetById<Order>(id);
        if (order == null)
            throw new EntityNotFoundException($"No se encontró una orden con ID {id}");

        // Verificamos si debemos reponer el stock
        // Solo reponemos si la orden estaba activa (PENDING, PROCESSING, SHIPPED)
        bool shouldRestock = order.Status != OrderStatus.CANCELLED && order.Status != OrderStatus.DELIVERED;

        var items = await _repository.Where<OrderItems>(i => i.OrderId == id);

        if (shouldRestock)
        {
            // Obtenemos los IDs de los productos para buscarlos
            var productIds = items.Select(i => i.ProductId).ToList();
            var products = await _repository.Where<Product>(p => productIds.Contains(p.Id));

            foreach (var item in items)
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product != null)
                {
                    // Devolvemos el stock al producto
                    product.StockQuantity += item.Quantity;
                    await _repository.Update(product);
                }
                // Borramos el item de la orden
                await _repository.Delete(item);
            }
        }
        else
        {
            // Si la orden ya estaba CANCELLED o DELIVERED, no reponemos stock
            // Solo borramos los items
            foreach (var item in items)
            {
                await _repository.Delete(item);
            }
        }

        // Finalmente, borramos la orden
        await _repository.Delete(order);
        _logger.LogInformation("Orden {OrderId} y sus items fueron eliminados. Reposición de stock: {Restock}", id, shouldRestock);
    }

    public async Task<PagedResult<OrderModel.Response>> GetOrdersByCustomerId(Guid customerId, int pageNumber, int pageSize)
    {
        // Filtramos por CustomerId
        var orders = await _repository.Where<Order>(o => o.CustomerId == customerId);

        if (!orders.Any())
            return new PagedResult<OrderModel.Response>(new List<OrderModel.Response>(), 0, pageNumber, 0);

        // Ordenamos por fecha descendente (las más nuevas primero)
        orders = orders.OrderByDescending(o => o.Date);

        // Paginación
        var totalCount = orders.Count();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        var skip = (pageNumber - 1) * pageSize;
        var ordersPag = orders.Skip(skip).Take(pageSize);

        // Obtenemos datos relacionados para armar la respuesta
        var orderItems = await _repository.GetAll<OrderItems>();
        var products = await _repository.GetAll<Product>();

        var responses = ordersPag.Select(order =>
        {
            var items = orderItems
                .Where(i => i.OrderId == order.Id)
                .Select(item =>
                {
                    var product = products.FirstOrDefault(p => p.Sku == item.SkuProd);
                    return new OrderModel.OrderItemResponse(
                        ProductId: product?.Id ?? Guid.Empty,
                        Name: product?.Name ?? "Producto desconocido",
                        Quantity: item.Quantity,
                        UnitPrice: item.UnitPrice,
                        Subtotal: item.Subtotal
                    );
                }).ToList();

            return new OrderModel.Response(
                Id: order.Id,
                CustomerId: order.CustomerId,
                Status: order.Status,
                TotalAmount: order.TotalAmount,
                Date: order.Date,
                ShippingAddress: order.ShippingAddress,
                BillingAddress: order.BillingAddress,
                Items: items
            );
        });

        return new PagedResult<OrderModel.Response>(
            Items: responses,
            TotalPages: totalPages,
            CurrentPage: pageNumber,
            TotalCount: totalCount
        );
    }
}
