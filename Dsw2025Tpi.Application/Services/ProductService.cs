using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Services
{
    public class ProductService
    {
        private readonly IRepository _repository;

        public ProductService(IRepository repository)
        {
            _repository = repository;
        }

        public async Task<ProductModel.Response?> GetProductById(Guid id)
        {
            var product = await _repository.GetById<Product>(id);
            if (product == null) return null;

            return new ProductModel.Response(
                product.Id,
                product.Sku,
                product.InternalCode,
                product.Name,
                product.Description,
                product.CurrentUnitPrice,
                product.StockQuantity,
                product.IsActive
            );
        }

        public async Task<IEnumerable<ProductModel.Response>?> GetProducts()
        {
            var products = await _repository.GetFiltered<Product>(p => p.IsActive);
            return products?.Select(p => new ProductModel.Response(
                p.Id,
                p.Sku,
                p.InternalCode,
                p.Name,
                p.Description,
                p.CurrentUnitPrice,
                p.StockQuantity,
                p.IsActive
            ));
        }

        public async Task<ProductModel.Response> AddProduct(ProductModel.Request request)
        {
            // Validaciones del enunciado
            if (string.IsNullOrWhiteSpace(request.Sku) ||
                string.IsNullOrWhiteSpace(request.Name) ||
                request.CurrentUnitPrice <= 0 ||
                request.StockQuantity < 0)
            {
                throw new ArgumentException("Valores para el producto no validos");
            }

            var exist = await _repository.First<Product>(p => p.Sku == request.Sku);
            if (exist != null)
                throw new DuplicatedEntityException($"Ya existe un producto con el SKU {request.Sku}");

            var product = new Product
            {
                
                Sku = request.Sku,
                InternalCode = request.InternalCode,
                Name = request.Name,
                Description = request.Description,
                CurrentUnitPrice = request.CurrentUnitPrice,
                StockQuantity = request.StockQuantity,
                IsActive = true
            };

            await _repository.Add(product);

            return new ProductModel.Response(
                product.Id,
                product.Sku,
                product.InternalCode,
                product.Name,
                product.Description,
                product.CurrentUnitPrice,
                product.StockQuantity,
                product.IsActive
            );
        }
    }
}
