using Product.Application.DTOs;
using Product.Application.Interfaces;
using Product.Domain.Entities;

namespace Product.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ProductDto>> GetProductsAsync(ProductFilterDto? filter = null)
    {
        var products = await _repository.GetAllAsync(filter);
        var dtos = products.Select(MapToDto).ToList();

        if (!string.IsNullOrEmpty(filter?.Sort))
        {
            dtos = filter.Sort.ToLower() switch
            {
                "price-asc" => dtos.OrderBy(p => p.Price).ToList(),
                "price-desc" => dtos.OrderByDescending(p => p.Price).ToList(),
                "rating" => dtos.OrderByDescending(p => p.Rating).ToList(),
                "newest" => dtos.OrderByDescending(p => p.CreatedAt).ToList(),
                _ => dtos
            };
        }

        return dtos;
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var product = await _repository.GetByIdAsync(id);
        return product == null ? null : MapToDto(product);
    }

    public async Task<ProductDto> CreateProductAsync(ProductCreateDto dto)
    {
        var entity = new ProductEntity
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Stock = dto.Stock,
            Category = dto.Category,
            Brand = dto.Brand,
            Rating = dto.Rating,
            ReviewCount = dto.ReviewCount,
            Image = dto.Image,
            Colors = dto.Colors != null ? string.Join(",", dto.Colors) : string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        var id = await _repository.CreateAsync(entity);
        entity.Id = id;
        return MapToDto(entity);
    }

    public async Task<bool> UpdateProductAsync(int id, ProductUpdateDto dto)
    {
        var entity = new ProductEntity
        {
            Id = id,
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Stock = dto.Stock,
            Category = dto.Category,
            Brand = dto.Brand,
            Rating = dto.Rating,
            ReviewCount = dto.ReviewCount,
            Image = dto.Image,
            Colors = dto.Colors != null ? string.Join(",", dto.Colors) : string.Empty
        };

        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    private static ProductDto MapToDto(ProductEntity p)
    {
        var colors = !string.IsNullOrWhiteSpace(p.Colors)
            ? p.Colors.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            : Array.Empty<string>();

        return new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            Stock = p.Stock,
            Category = p.Category,
            Brand = p.Brand,
            Rating = p.Rating,
            ReviewCount = p.ReviewCount,
            Image = p.Image ?? "https://picsum.photos/seed/default/640/640",
            Colors = colors,
            CreatedAt = p.CreatedAt.ToString("yyyy-MM-dd")
        };
    }
}
