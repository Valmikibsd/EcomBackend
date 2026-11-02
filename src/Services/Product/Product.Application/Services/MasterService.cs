using Product.Application.DTOs;
using Product.Application.Interfaces;

namespace Product.Application.Services;

public class MasterService : IMasterService
{
    private readonly IMasterRepository _masterRepository;

    public MasterService(IMasterRepository masterRepository)
    {
        _masterRepository = masterRepository;
    }

    public async Task<IEnumerable<CategoryDto>> GetCategoriesAsync()
    {
        var entities = await _masterRepository.GetCategoriesAsync();
        return entities.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            CreatedAt = c.CreatedAt.ToString("yyyy-MM-dd")
        });
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Category name is required.");

        var id = await _masterRepository.CreateCategoryAsync(dto.Name.Trim(), dto.Description?.Trim());
        if (id == -1)
            throw new InvalidOperationException($"Category '{dto.Name}' already exists.");

        return new CategoryDto
        {
            Id = id,
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd")
        };
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        return await _masterRepository.DeleteCategoryAsync(id);
    }

    public async Task<IEnumerable<BrandDto>> GetBrandsAsync()
    {
        var entities = await _masterRepository.GetBrandsAsync();
        return entities.Select(b => new BrandDto
        {
            Id = b.Id,
            Name = b.Name,
            Description = b.Description,
            CreatedAt = b.CreatedAt.ToString("yyyy-MM-dd")
        });
    }

    public async Task<BrandDto> CreateBrandAsync(CreateBrandDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Brand name is required.");

        var id = await _masterRepository.CreateBrandAsync(dto.Name.Trim(), dto.Description?.Trim());
        if (id == -1)
            throw new InvalidOperationException($"Brand '{dto.Name}' already exists.");

        return new BrandDto
        {
            Id = id,
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd")
        };
    }

    public async Task<bool> DeleteBrandAsync(int id)
    {
        return await _masterRepository.DeleteBrandAsync(id);
    }
}
