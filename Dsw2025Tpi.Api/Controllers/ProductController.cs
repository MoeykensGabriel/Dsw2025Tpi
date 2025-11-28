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

namespace Dsw2025Tpi.Api.Controllers;

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
    [AllowAnonymous]
    [Authorize]
    public async Task<IActionResult> GetAllProducts(
        [FromQuery] int pageSize=8,
        [FromQuery] int pageNumber = 1,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null)
    {
        // el servicio ahora devuelve un objeto PagedResult<Product>
        var pagedResult = await _productsManagementService.GetAllProducts(pageSize, pageNumber, search, status);
        // devolvemos el objeto completo
        return Ok(pagedResult);
    }

    [HttpGet("total")]
    [AllowAnonymous]
    [Authorize]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _productsManagementService.GetProducts();
        return Ok(products);
    }

    [HttpGet("summary")]
    [AllowAnonymous]
    [Authorize]
    public async Task<IActionResult> GetSummary()
    {
        var summary = await _productsManagementService.GetProductSummary();
        return Ok(summary);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize]
    public async Task<IActionResult> GetProductById(Guid id)
    {
        var products = await _productsManagementService.GetProductById(id);
        return Ok(products);
    }

    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<IActionResult> GetActiveProducts(
    [FromQuery] int pageSize = 8,
    [FromQuery] int pageNumber = 1,
    [FromQuery] string? search = null,
    [FromQuery] decimal? minPrice = null,
    [FromQuery] decimal? maxPrice = null
)
    {
        var pagedResult = await _productsManagementService.GetActiveProducts(
            pageSize,
            pageNumber,
            search,
            minPrice,
            maxPrice
        );

        return Ok(pagedResult);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddProduct([FromBody] ProductModel.Request data)
    {
        var product = await _productsManagementService.AddProduct(data);
        return Created($"/api/products/{product.Id}", product);
    }

    [HttpPut("{id}")] // debo poner el id en un update?
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] ProductModel.Request product)
    {
        var updateProduct = await _productsManagementService.UpdateProduct(id, product);
        return Ok(updateProduct);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        await _productsManagementService.DeleteProduct(id);
        return NoContent();
    }

    [HttpPatch("{id}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DisableProduct(Guid id)
    {
        await _productsManagementService.DisableProduct(id);
        return NoContent();
    }
}

