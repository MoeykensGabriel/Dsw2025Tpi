using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using Dsw2025Tpi.Application.Dtos;
using static Dsw2025Tpi.Application.Dtos.ProductModel;
using System.Data;
using Dsw2025Tpi.Application.Exceptions;
using System.ComponentModel;

namespace Dsw2025Tpi.Application.Services;

public class ProductsManagementService
{
    private readonly IRepository _repository;

    public ProductsManagementService(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductModel.Response> AddProduct(ProductModel.Request product)
    {
        if (string.IsNullOrWhiteSpace(product.Sku) || string.IsNullOrWhiteSpace(product.InternalCode) ||
            string.IsNullOrWhiteSpace(product.Name) || string.IsNullOrWhiteSpace(product.Description))
        {
            throw new BadRequestException("Faltan datos del producto a llenar.");
        }

        if (product.StockQuantity < 0 || product.CurrentUnitPrice <= 0)
        {
            throw new BadRequestException("Cantidades de Stock y/o Precio no validos para un producto.");
        }

        var productFound = await _repository.First<Product>(p => p.Sku == product.Sku);
        if (productFound != null)
        {
            throw new DuplicatedEntityException($"Producto con Sku {product.Sku} ya existente.");
        }

        var productAdd = new Product(product.Sku, product.Name, product.Description, product.InternalCode, (int)product.CurrentUnitPrice, (int)product.StockQuantity);
        await _repository.Add(productAdd);

        return new ProductModel.Response(
             productAdd.Id,
             productAdd.Sku!,
             productAdd.InternalCode!,
             productAdd.Name!,
             productAdd.Description!,
             productAdd.CurrentUnitPrice,
             productAdd.StockQuantity,
             productAdd.IsActive
        );
    }

    public async Task<IEnumerable<Product>?> GetAllProducts()
    {
        IEnumerable<Product> products = await _repository.GetAll<Product>();

        if (products == null)
        {
            throw new EntityNotFoundException("Ningun producto cargado/disponible.");
        }
        else
        {
            return products;
        }
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

    public async Task UpdateProduct(Guid id, ProductModel.Request product)
    {
        var productById = await _repository.GetById<Product>(id);

        if (productById != null)
        {
            if (await _repository.First<Product>(p => p.Sku == product.Sku && p.Id != id) == null)
            {

                if (product.Sku != null)
                {
                    if (!(string.IsNullOrWhiteSpace(product.Sku)))
                    {
                        productById.Sku = product.Sku;
                    }
                    else
                    {
                        throw new BadRequestException("Sku del producto inexistente.");
                    }
                }

                if (product.Name != null)
                {
                    if (!(string.IsNullOrWhiteSpace(product.Name)))
                    {
                        productById.Name = product.Name;
                    }
                    else
                    {
                        throw new BadRequestException("Sku del producto inexistente.");
                    }
                }

                if (product.InternalCode != null)
                {
                    if (!(string.IsNullOrWhiteSpace(product.InternalCode)))
                    {
                        productById.InternalCode = product.InternalCode;
                    }
                    else
                    {
                        throw new BadRequestException("Sku del producto inexistente.");
                    }
                }

                if (product.Description != null)
                {
                    if (!(string.IsNullOrWhiteSpace(product.Description)))
                    {
                        productById.Description = product.Description;
                    }
                    else
                    {
                        throw new BadRequestException("Sku del producto inexistente.");
                    }
                }

                if (product.StockQuantity != null)
                {
                    if (!(product.StockQuantity < 0))
                    {
                        productById.StockQuantity = (int)product.StockQuantity;
                    }
                    else
                    {
                        throw new BadRequestException("Cantidad de stock menor a cero.");
                    }
                }

                if (product.CurrentUnitPrice != null)
                {
                    if (!(product.CurrentUnitPrice <= 0))
                    {
                        productById.CurrentUnitPrice = (decimal)product.CurrentUnitPrice;
                    }
                    else
                    {
                        throw new BadRequestException("Error: Valor del precio unitario menor/igual a cero.");
                    }
                }

                await _repository.Update<Product>(productById);
            }
            else
            {
                throw new DuplicatedEntityException($"Producto con el Sku a modificar ( {product.Sku} ) encontrado en otro producto existente.");
            }
        }
        else
        {
            throw new EntityNotFoundException("Producto a actualizar no cargado/disponible.");
        }
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


