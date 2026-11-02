using System.Data;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Dapper;
using SharedKernel.Data;

namespace Auth.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<User?> GetByUsernameOrEmailAsync(string identifier)
    {
        using var conn = _connectionFactory.CreateConnection();
        // Stored procedure invocation with fallback to table query
        try
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Identifier", identifier);
            return await conn.QueryFirstOrDefaultAsync<User>(
                "USP_User_GetByUsernameOrEmail",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }
        catch
        {
            const string sql = @"
                SELECT TOP 1 [Id], [Username], [Email], [Password], [Role], [RefreshToken], [RefreshTokenExpiryTime]
                FROM [Users]
                WHERE [Username] = @Identifier OR [Email] = @Identifier";

            return await conn.QueryFirstOrDefaultAsync<User>(sql, new { Identifier = identifier });
        }
    }

    public async Task<User?> ValidateUserLoginProcAsync(string username, string password)
    {
        using var conn = _connectionFactory.CreateConnection();
        // Calling stored procedure USP_User_Login
        var parameters = new DynamicParameters();
        parameters.Add("@Username", username);
        parameters.Add("@Password", password);

        return await conn.QueryFirstOrDefaultAsync<User>(
            "USP_User_Login",
            parameters,
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<int> RegisterUserProcAsync(string username, string email, string password, string role)
    {
        using var conn = _connectionFactory.CreateConnection();
        // Calling stored procedure USP_User_Register
        var parameters = new DynamicParameters();
        parameters.Add("@Username", username);
        parameters.Add("@Email", email);
        parameters.Add("@Password", password);
        parameters.Add("@Role", role);

        return await conn.ExecuteScalarAsync<int>(
            "USP_User_Register",
            parameters,
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task UpdateRefreshTokenProcAsync(int userId, string refreshToken, DateTime expiryTime)
    {
        using var conn = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@UserId", userId);
        parameters.Add("@RefreshToken", refreshToken);
        parameters.Add("@RefreshTokenExpiryTime", expiryTime);

        await conn.ExecuteAsync(
            "USP_User_UpdateRefreshToken",
            parameters,
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<User?> GetByRefreshTokenProcAsync(string refreshToken)
    {
        using var conn = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@RefreshToken", refreshToken);

        return await conn.QueryFirstOrDefaultAsync<User>(
            "USP_User_GetByRefreshToken",
            parameters,
            commandType: CommandType.StoredProcedure
        );
    }
}
