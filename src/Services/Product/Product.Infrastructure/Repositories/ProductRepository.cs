using System.Data;
using Dapper;
using Product.Application.DTOs;
using Product.Application.Interfaces;
using Product.Domain.Entities;
using SharedKernel.Data;

namespace Product.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ProductRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<ProductEntity>> GetAllAsync(ProductFilterDto? filter = null)
    {
        using var conn = _connectionFactory.CreateConnection();
        try
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Search", string.IsNullOrWhiteSpace(filter?.Search) ? null : filter.Search.Trim());
            parameters.Add("@Category", string.IsNullOrWhiteSpace(filter?.Category) ? null : filter.Category.Trim());
            parameters.Add("@Brand", string.IsNullOrWhiteSpace(filter?.Brand) ? null : filter.Brand.Trim());
            parameters.Add("@MinPrice", filter?.MinPrice);
            parameters.Add("@MaxPrice", filter?.MaxPrice);
            parameters.Add("@InStockOnly", filter?.InStockOnly ?? false);

            return await conn.QueryAsync<ProductEntity>(
                "USP_Product_GetAll",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }
        catch
        {
            const string sql = @"
                SELECT [Id], [Name], [Description], [Price], [Stock], [Category], [Brand], [Rating], [ReviewCount], [Image], [Colors], [CreatedAt]
                FROM [Products]
                WHERE (@Search IS NULL OR [Name] LIKE '%' + @Search + '%' OR [Description] LIKE '%' + @Search + '%')
                  AND (@Category IS NULL OR [Category] = @Category)
                  AND (@Brand IS NULL OR [Brand] = @Brand)
                  AND (@MinPrice IS NULL OR [Price] >= @MinPrice)
                  AND (@MaxPrice IS NULL OR [Price] <= @MaxPrice)
                  AND (@InStockOnly = 0 OR [Stock] > 0)
                ORDER BY [Id] ASC";

            return await conn.QueryAsync<ProductEntity>(sql, new
            {
                Search = string.IsNullOrWhiteSpace(filter?.Search) ? null : filter.Search.Trim(),
                Category = string.IsNullOrWhiteSpace(filter?.Category) ? null : filter.Category.Trim(),
                Brand = string.IsNullOrWhiteSpace(filter?.Brand) ? null : filter.Brand.Trim(),
                MinPrice = filter?.MinPrice,
                MaxPrice = filter?.MaxPrice,
                InStockOnly = filter?.InStockOnly ?? false
            });
        }
    }

    public async Task<ProductEntity?> GetByIdAsync(int id)
    {
        using var conn = _connectionFactory.CreateConnection();
        try
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            return await conn.QueryFirstOrDefaultAsync<ProductEntity>(
                "USP_Product_GetById",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }
        catch
        {
            const string sql = @"
                SELECT [Id], [Name], [Description], [Price], [Stock], [Category], [Brand], [Rating], [ReviewCount], [Image], [Colors], [CreatedAt]
                FROM [Products]
                WHERE [Id] = @Id";

            return await conn.QueryFirstOrDefaultAsync<ProductEntity>(sql, new { Id = id });
        }
    }

    public async Task<int> CreateAsync(ProductEntity product)
    {
        using var conn = _connectionFactory.CreateConnection();
        try
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Name", product.Name);
            parameters.Add("@Description", product.Description);
            parameters.Add("@Price", product.Price);
            parameters.Add("@Stock", product.Stock);
            parameters.Add("@Category", product.Category);
            parameters.Add("@Brand", product.Brand);
            parameters.Add("@Rating", product.Rating);
            parameters.Add("@ReviewCount", product.ReviewCount);
            parameters.Add("@Image", product.Image);
            parameters.Add("@Colors", product.Colors);

            return await conn.ExecuteScalarAsync<int>(
                "USP_Product_Insert",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }
        catch
        {
            const string sql = @"
                INSERT INTO [Products] ([Name], [Description], [Price], [Stock], [Category], [Brand], [Rating], [ReviewCount], [Image], [Colors], [CreatedAt])
                VALUES (@Name, @Description, @Price, @Stock, @Category, @Brand, @Rating, @ReviewCount, @Image, @Colors, GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            return await conn.ExecuteScalarAsync<int>(sql, product);
        }
    }

    public async Task<bool> UpdateAsync(ProductEntity product)
    {
        using var conn = _connectionFactory.CreateConnection();
        try
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", product.Id);
            parameters.Add("@Name", product.Name);
            parameters.Add("@Description", product.Description);
            parameters.Add("@Price", product.Price);
            parameters.Add("@Stock", product.Stock);
            parameters.Add("@Category", product.Category);
            parameters.Add("@Brand", product.Brand);
            parameters.Add("@Rating", product.Rating);
            parameters.Add("@ReviewCount", product.ReviewCount);
            parameters.Add("@Image", product.Image);
            parameters.Add("@Colors", product.Colors);

            var rows = await conn.ExecuteAsync(
                "USP_Product_Update",
                parameters,
                commandType: CommandType.StoredProcedure
            );
            return rows > 0;
        }
        catch
        {
            const string sql = @"
                UPDATE [Products]
                SET [Name] = @Name,
                    [Description] = @Description,
                    [Price] = @Price,
                    [Stock] = @Stock,
                    [Category] = @Category,
                    [Brand] = @Brand,
                    [Rating] = @Rating,
                    [ReviewCount] = @ReviewCount,
                    [Image] = @Image,
                    [Colors] = @Colors
                WHERE [Id] = @Id";

            var rows = await conn.ExecuteAsync(sql, product);
            return rows > 0;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = _connectionFactory.CreateConnection();
        try
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            var rows = await conn.ExecuteAsync(
                "USP_Product_Delete",
                parameters,
                commandType: CommandType.StoredProcedure
            );
            return rows > 0;
        }
        catch
        {
            const string sql = "DELETE FROM [Products] WHERE [Id] = @Id";
            var rows = await conn.ExecuteAsync(sql, new { Id = id });
            return rows > 0;
        }
    }
}
