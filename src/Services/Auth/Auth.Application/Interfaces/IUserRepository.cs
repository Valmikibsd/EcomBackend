using Auth.Domain.Entities;

namespace Auth.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByUsernameOrEmailAsync(string identifier);
    Task<User?> ValidateUserLoginProcAsync(string username, string password);
    Task<int> RegisterUserProcAsync(string username, string email, string password, string role);
    Task UpdateRefreshTokenProcAsync(int userId, string refreshToken, DateTime expiryTime);
    Task<User?> GetByRefreshTokenProcAsync(string refreshToken);
}
