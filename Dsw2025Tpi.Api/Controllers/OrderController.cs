using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2025Tpi.Api.Controllers
{
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
        public async Task<IActionResult> GetAllOrders()
        {
            try
            {
                var orders = await _ordersManagementService.GetAllOrders();
                return Ok(orders);
            }
            catch (EntityNotFoundException)
            {
                return NoContent();
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddOrder([FromBody] OrderModel.Request data)
        {
            try
            {
                var order = await _ordersManagementService.addOrder(data);
                return Created($"/api/orders/{order.Id}", order);
            }
            catch (DuplicatedEntityException)
            {
                return BadRequest("Error: Orden duplicada.");
            }
            catch (EntityNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteOrder(Guid id)
        {
            try
            {
                await _ordersManagementService.DeleteOrder(id);
                return Ok("Orden eliminada exitosamente de la base de datos.");
            }
            catch (EntityNotFoundException)
            {
                return NotFound($"No hay orden con ID {id}");
            }
        }
    }

}
