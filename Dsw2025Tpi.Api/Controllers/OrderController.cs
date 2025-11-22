using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

    [HttpGet("{id}")]
    [Authorize] // Protegido para que solo usuarios logueados (admin o user) puedan ver
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        var order = await _ordersManagementService.GetOrderById(id);
        return Ok(order);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllOrders(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 8,
        [FromQuery] string? status = null,
        [FromQuery] string? search = null) 
    {
        var orders = await _ordersManagementService.GetAllOrders(pageNumber, pageSize, status, search);
        return Ok(orders);
    }

    [HttpGet("summary")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetOrdersSummary()
    {
        var summary = await _ordersManagementService.GetOrdersSummary();
        return Ok(summary);
    }


    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize]
    public async Task<IActionResult> AddOrder([FromBody] OrderModel.Request data)
    {
        var order = await _ordersManagementService.addOrder(data);
        return Created($"/api/orders/{order.Id}", order);
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] string newStatus)
    {
        await _ordersManagementService.UpdateOrderStatus(id, newStatus);
        return Ok($"Se modifico el estado de la orden {id} a {newStatus}");
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteOrder(Guid id)
    {
        await _ordersManagementService.DeleteOrder(id);
        return NoContent(); // 204 No Content es la respuesta estándar para un DELETE exitoso
    }

    [HttpGet("my-orders")]
    [Authorize]
    public async Task<IActionResult> GetMyOrders(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        Guid userId = Guid.Empty;
        bool found = false;

        // 1. Filtramos SOLO los claims que sean 'NameIdentifier' ('sub' o el string largo)
        var idClaims = User.Claims.Where(c =>
            c.Type == ClaimTypes.NameIdentifier ||
            c.Type == "sub" ||
            c.Type == "id"
        );

        // 2. De esos, buscamos el que sea un GUID válido
        foreach (var claim in idClaims)
        {
            if (Guid.TryParse(claim.Value, out userId))
            {
                found = true;
                break; // ¡Encontramos el ID de usuario real!
            }
        }

        if (!found)
        {
            return BadRequest(new
            {
                code = "INVALID_TOKEN",
                message = "No se pudo encontrar un ID de usuario válido en el token."
            });
        }

        var result = await _ordersManagementService.GetOrdersByCustomerId(userId, pageNumber, pageSize);

        return Ok(result);
    }
}
