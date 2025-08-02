using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2025Tpi.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrderController : ControllerBase
{
    private readonly OrdersManagementService _ordersManagementService;

    public OrderController(OrdersManagementService ordersManagementService)
    {
        _ordersManagementService = ordersManagementService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllOrders()
    {
        var orders = await _ordersManagementService.GetAllOrders();
        return Ok(orders);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> AddOrder([FromBody] OrderModel.Request data)
    {
        var order = await _ordersManagementService.addOrder(data);
        return Created($"/api/orders/{order.Id}", order);
    }

    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteOrder(Guid id)
    {
        await _ordersManagementService.DeleteOrder(id);
        return Ok("Orden eliminada exitosamente de la base de datos.");
    }
}


