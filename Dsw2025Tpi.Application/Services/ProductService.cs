using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using Dsw2025Tpi.Application.Dtos;
using System.Data;
using Dsw2025Tpi.Application.Exceptions;
using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Dsw2025Tpi.Data.Migrations;

namespace Dsw2025Tpi.Application.Services;

public class ProductsManagementService
{
    private readonly IRepository _repository;
    private readonly ILogger<ProductsManagementService> _logger;

    public ProductsManagementService(IRepository repository, ILogger<ProductsManagementService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ProductModel.Response> AddProduct(ProductModel.Request product)
    {
        if (string.IsNullOrWhiteSpace(product.Sku) || string.IsNullOrWhiteSpace(product.InternalCode) ||
            string.IsNullOrWhiteSpace(product.Name) || string.IsNullOrWhiteSpace(product.Description))
            throw new BadRequestException("Faltan datos del producto a llenar.");

        if (product.StockQuantity < 0 || product.CurrentUnitPrice <= 0)
            throw new BadRequestException("Cantidades de Stock y/o Precio no validos para un producto.");

        var productFound = await _repository.First<Product>(p => p.Sku == product.Sku);

        if (productFound != null)
            throw new DuplicatedEntityException($"Producto con Sku {product.Sku} ya existe.");

        var productAdd = new Product(product.Sku, product.Name, product.Description,
             product.InternalCode, product.CurrentUnitPrice, product.StockQuantity, product.ImageUrl);

        await _repository.Add(productAdd);

        _logger.LogInformation("Se creo un producto con: ID={Id}, nombre={Nombre}, SKU={Sku}",
            productAdd.Id,productAdd.Name,productAdd.Sku);

        return new ProductModel.Response(
             productAdd.Id,
             productAdd.Sku!,
             productAdd.InternalCode!,
             productAdd.Name!,
             productAdd.Description!,
             productAdd.CurrentUnitPrice,
             productAdd.StockQuantity,
             productAdd.IsActive,
             productAdd.ImageUrl
        );
    }

    public async Task<PagedResult<Product>> GetAllProducts(int pageSize = 8, int pageNumber = 1, string? search = null)
    {
        IEnumerable<Product> products = await _repository.GetAll<Product>();

        if (products == null || !products.Any())
            throw new EntityNotFoundException("No hay productos cargados.");
        //Logica de filtrado
        if (!string.IsNullOrEmpty(search))
        {
            var searchTerm = search.ToLowerInvariant().Trim();
            products = products.Where(p =>
               (p.Name != null && p.Name.ToLowerInvariant().Contains(searchTerm)) ||
                (p.Sku != null && p.Sku.ToLowerInvariant().Contains(searchTerm)) ||
                (p.Description != null && p.Description.ToLowerInvariant().Contains(searchTerm))
            );
        }
        //Calcular el total ANTES de paginar
        var totalCount = products.Count();

        //Calcular totalPages
        // (Usamos Math.Ceiling para redondear hacia arriba)
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var skip = (pageNumber - 1) * pageSize;
        var productsPag = products.Skip(skip).Take(pageSize);

        _logger.LogInformation("Se listaron {Count} productos" +
            " en pagina {pNumber} con tamaño de pagina {pSize}", productsPag.Count(), pageNumber, pageSize);

        return new PagedResult<Product>(
            Items: productsPag,
            TotalPages: totalPages,
            CurrentPage: pageNumber,
            TotalCount: totalCount
        );
    }

    public async Task<PagedResult<Product>> GetActiveProducts(int pageSize=8, int pageNumber=1, string? search = null)
    {
        IEnumerable<Product> products = await _repository.GetFiltered<Product>(p => p.IsActive);


        if (products == null || !products.Any())
            throw new EntityNotFoundException("No hay productos cargados.");

        //Logica de filtrado
        if (!string.IsNullOrEmpty(search))
        {
            var searchTerm = search.ToLowerInvariant().Trim();
            products = products.Where(p =>
               (p.Name != null && p.Name.ToLowerInvariant().Contains(searchTerm)) ||
                (p.Sku != null && p.Sku.ToLowerInvariant().Contains(searchTerm)) ||
                (p.Description != null && p.Description.ToLowerInvariant().Contains(searchTerm))
            );
        }

        
        //Calcular el total ANTES de paginar
        var totalCount = products.Count();

        //Calcular totalPages
        // (Usamos Math.Ceiling para redondear hacia arriba)
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var skip = (pageNumber - 1) * pageSize;
        var productsPag = products.Skip(skip).Take(pageSize);

        _logger.LogInformation("Se listaron {Count} productos ACTIVOS" +
            " en pagina {pNumber} con tamaño de pagina {pSize}", productsPag.Count(), pageNumber, pageSize);

        return new PagedResult<Product>(
            Items: productsPag,
            TotalPages: totalPages,
            CurrentPage: pageNumber,
            TotalCount: totalCount
        );
    }

    public async Task<Product?> GetProductById(Guid id)
    {
        var productById = await _repository.GetById<Product>(id);
        if (productById != null)
        {
            return productById;
        }
        else
        {
            throw new EntityNotFoundException($"Ningun producto con ID {id} cargado/disponible.");
        }
    }

    public async Task<ProductModel.Response> UpdateProduct(Guid id, ProductModel.Request product)
    {
        var productById = await _repository.GetById<Product>(id);

        if (productById == null)
            throw new EntityNotFoundException("Producto a actualizar No Cargado");

        if (await _repository.First<Product>(p => p.Sku == product.Sku && p.Id != id) != null)
            throw new DuplicatedEntityException($"Producto con el Sku={product.Sku} a modificar ya esta asociado a otro producto");

        if (product.Sku != null)
        {
            if (!string.IsNullOrWhiteSpace(product.Sku))
                productById.Sku = product.Sku;
            else
                throw new BadRequestException("Sku del producto no existe");
        }

        if (product.Name != null)
        {
            if (!string.IsNullOrWhiteSpace(product.Name))
                productById.Name = product.Name;
            else
                throw new BadRequestException("Nombre del producto no existe");
        }

        if (product.InternalCode != null)
        {
            if (!string.IsNullOrWhiteSpace(product.InternalCode))
                productById.InternalCode = product.InternalCode;
            else
                throw new BadRequestException("Codigo interno del producto no existe");
        }

        if (product.Description != null)
        {
            if (!string.IsNullOrWhiteSpace(product.Description))
                productById.Description = product.Description;
            else
                throw new BadRequestException("Descripcion del producto no existe");
        }

        if (product.StockQuantity != null)
        {
            if (product.StockQuantity >= 0)
                productById.StockQuantity = (int)product.StockQuantity;
            else
                throw new BadRequestException("Cantidad de stock menor a cero.");
        }

        if (product.CurrentUnitPrice != null)
        {
            if (product.CurrentUnitPrice > 0)
                productById.CurrentUnitPrice = (decimal)product.CurrentUnitPrice;
            else
                throw new BadRequestException("Error: Valor del precio unitario menor/igual a cero.");
        }

        if (product.ImageUrl != null)
        {
            productById.ImageUrl = string.IsNullOrWhiteSpace(product.ImageUrl) ? null : product.ImageUrl;
        }

        await _repository.Update<Product>(productById);

        _logger.LogInformation("Producto con el id={Id} actualizado correctamente ", productById.Id);

        return new ProductModel.Response
            (
             productById.Id,
             productById.Sku!,
             productById.InternalCode!,
             productById.Name!,
             productById.Description!,
             productById.CurrentUnitPrice,
             productById.StockQuantity,
             productById.IsActive,
             productById.ImageUrl

            );
    }

    public async Task DeleteProduct(Guid id)
    {
        var productById = await _repository.GetById<Product>(id);

        if (productById == null)
        {
            throw new EntityNotFoundException("Producto a eliminar no cargado/disponible.");
        }

        var orderItems = await _repository.Where<OrderItems>(oi => oi.ProductId == id);
        if (orderItems.Any())
        {
            throw new ConflictException("No se puede eliminar el producto porque está asociado a una orden.");
        }

        await _repository.Delete(productById);
    }

    public async Task DisableProduct(Guid id)
    {
        var productById = await _repository.GetById<Product>(id);
        if (productById != null)
        {
            productById.IsActive = false;
            await _repository.Update(productById);
        }
        else
        {
            throw new EntityNotFoundException("Producto a inhabilitar no cargado/disponible.");
        }
    }
}


