using Product.Application.DTOs;

namespace Product.Application.Interfaces;

public interface IMasterService
{
    Task<IEnumerable<CategoryDto>> GetCategoriesAsync();
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto);
    Task<bool> DeleteCategoryAsync(int id);

    Task<IEnumerable<BrandDto>> GetBrandsAsync();
    Task<BrandDto> CreateBrandAsync(CreateBrandDto dto);
    Task<bool> DeleteBrandAsync(int id);
}
