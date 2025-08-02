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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize]
    public async Task<IActionResult> GetAllProducts()
    {
        var products = await _productsManagementService.GetAllProducts();
        return Ok(products);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize]
    public async Task<IActionResult> GetProductById(Guid id)
    {
        var products = await _productsManagementService.GetProductById(id);
        return Ok(products);
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
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] ProductModel.Request data)
    {
        await _productsManagementService.UpdateProduct(id, data);
        return Ok("Producto modificado con exito.");
    }

    [HttpPatch]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        await _productsManagementService.DeleteProduct(id);
        return Ok("Producto eliminado con exito de la base de datos.");
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

