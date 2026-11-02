using Order.Application.DTOs;
using Order.Domain.Entities;

namespace Order.Application.Interfaces;

public interface IOrderRepository
{
    Task<int> CreateAsync(OrderEntity order);
    Task<OrderEntity?> GetByIdAsync(int id);
    Task<IEnumerable<OrderEntity>> GetByUserIdAsync(int userId);
    Task<IEnumerable<OrderEntity>> GetAllAsync();
    Task<bool> UpdateStatusAsync(int id, string status);
    Task<bool> DeleteAsync(int id);
}
