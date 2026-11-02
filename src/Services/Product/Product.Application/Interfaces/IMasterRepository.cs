using Product.Domain.Entities;

namespace Product.Application.Interfaces;

public interface IMasterRepository
{
    Task<IEnumerable<CategoryEntity>> GetCategoriesAsync();
    Task<int> CreateCategoryAsync(string name, string? description);
    Task<bool> DeleteCategoryAsync(int id);

    Task<IEnumerable<BrandEntity>> GetBrandsAsync();
    Task<int> CreateBrandAsync(string name, string? description);
    Task<bool> DeleteBrandAsync(int id);
}
