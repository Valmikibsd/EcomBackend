using Product.Application.DTOs;
using Product.Domain.Entities;

namespace Product.Application.Interfaces;

public interface IProductRepository
{
    Task<IEnumerable<ProductEntity>> GetAllAsync(ProductFilterDto? filter = null);
    Task<ProductEntity?> GetByIdAsync(int id);
    Task<int> CreateAsync(ProductEntity product);
    Task<bool> UpdateAsync(ProductEntity product);
    Task<bool> DeleteAsync(int id);
}
