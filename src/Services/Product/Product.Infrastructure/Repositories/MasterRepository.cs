using System.Data;
using Dapper;
using Product.Application.Interfaces;
using Product.Domain.Entities;
using SharedKernel.Data;

namespace Product.Infrastructure.Repositories;

public class MasterRepository : IMasterRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public MasterRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<CategoryEntity>> GetCategoriesAsync()
    {
        using var conn = _connectionFactory.CreateConnection();
        try
        {
            return await conn.QueryAsync<CategoryEntity>(
                "USP_Category_GetAll",
                commandType: CommandType.StoredProcedure
            );
        }
        catch
        {
            const string sql = "SELECT [Id], [Name], [Description], [CreatedAt] FROM [Categories] ORDER BY [Name] ASC";
            return await conn.QueryAsync<CategoryEntity>(sql);
        }
    }

    public async Task<int> CreateCategoryAsync(string name, string? description)
    {
        using var conn = _connectionFactory.CreateConnection();
        try
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Name", name);
            parameters.Add("@Description", description);

            return await conn.ExecuteScalarAsync<int>(
                "USP_Category_Insert",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }
        catch
        {
            const string sql = @"
                IF EXISTS (SELECT 1 FROM [Categories] WHERE [Name] = @Name)
                    SELECT -1;
                ELSE
                BEGIN
                    INSERT INTO [Categories] ([Name], [Description], [CreatedAt]) VALUES (@Name, @Description, GETDATE());
                    SELECT CAST(SCOPE_IDENTITY() AS INT);
                END";
            return await conn.ExecuteScalarAsync<int>(sql, new { Name = name, Description = description });
        }
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        using var conn = _connectionFactory.CreateConnection();
        try
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            var rows = await conn.ExecuteAsync(
                "USP_Category_Delete",
                parameters,
                commandType: CommandType.StoredProcedure
            );
            return rows > 0;
        }
        catch
        {
            const string sql = "DELETE FROM [Categories] WHERE [Id] = @Id";
            var rows = await conn.ExecuteAsync(sql, new { Id = id });
            return rows > 0;
        }
    }

    public async Task<IEnumerable<BrandEntity>> GetBrandsAsync()
    {
        using var conn = _connectionFactory.CreateConnection();
        try
        {
            return await conn.QueryAsync<BrandEntity>(
                "USP_Brand_GetAll",
                commandType: CommandType.StoredProcedure
            );
        }
        catch
        {
            const string sql = "SELECT [Id], [Name], [Description], [CreatedAt] FROM [Brands] ORDER BY [Name] ASC";
            return await conn.QueryAsync<BrandEntity>(sql);
        }
    }

    public async Task<int> CreateBrandAsync(string name, string? description)
    {
        using var conn = _connectionFactory.CreateConnection();
        try
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Name", name);
            parameters.Add("@Description", description);

            return await conn.ExecuteScalarAsync<int>(
                "USP_Brand_Insert",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }
        catch
        {
            const string sql = @"
                IF EXISTS (SELECT 1 FROM [Brands] WHERE [Name] = @Name)
                    SELECT -1;
                ELSE
                BEGIN
                    INSERT INTO [Brands] ([Name], [Description], [CreatedAt]) VALUES (@Name, @Description, GETDATE());
                    SELECT CAST(SCOPE_IDENTITY() AS INT);
                END";
            return await conn.ExecuteScalarAsync<int>(sql, new { Name = name, Description = description });
        }
    }

    public async Task<bool> DeleteBrandAsync(int id)
    {
        using var conn = _connectionFactory.CreateConnection();
        try
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            var rows = await conn.ExecuteAsync(
                "USP_Brand_Delete",
                parameters,
                commandType: CommandType.StoredProcedure
            );
            return rows > 0;
        }
        catch
        {
            const string sql = "DELETE FROM [Brands] WHERE [Id] = @Id";
            var rows = await conn.ExecuteAsync(sql, new { Id = id });
            return rows > 0;
        }
    }
}
