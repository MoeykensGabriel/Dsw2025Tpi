using Microsoft.AspNetCore.Mvc;
using Dsw2025Tpi.Application.Services;
using System.Threading.Tasks;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.Extensions.FileProviders.Physical;
using System.Reflection.Metadata;

namespace Dsw2025Tpi.Api.Controllers
{
    [ApiController]
    [Route("api/products")]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly ProductsManagementService _productsManagementService;

        public ProductController(ProductsManagementService productsManagementService)
        {
            _productsManagementService = productsManagementService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status204NoContent)]

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllProducts()
        {
            try
            {
                var products = await _productsManagementService.GetAllProducts();
                return Ok(products);
            }
            catch (EntityNotFoundException)
            {
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }

        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            try
            {
                var products = await _productsManagementService.GetProductById(id);
                return Ok(products);
            }
            catch (EntityNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddProduct([FromBody] ProductModel.Request data)
        {
            try
            {
                var product = await _productsManagementService.AddProduct(data);
                return Created($"/api/products/{product.Id}", product);
            }
            catch (DuplicatedEntityException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")] // debo poner el id en un update?
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] ProductModel.Request data)
        {
            try
            {
                await _productsManagementService.UpdateProduct(id, data);
                return Ok("Producto modificado con exito.");
            }
            catch (DuplicatedEntityException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (EntityNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            try
            {
                await _productsManagementService.DeleteProduct(id);
                return Ok("Producto eliminado con exito de la base de datos.");
            }
            catch (EntityNotFoundException ex)
            {
                return NotFound($"No hay producto con ID {id}");
            }
            catch(ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DisableProduct(Guid id)
        {
            try
            {
                await _productsManagementService.DisableProduct(id);
                return NoContent();
            }
            catch (EntityNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
