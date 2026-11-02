using Order.Application.DTOs;

namespace Order.Application.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(CreateOrderDto dto);
    Task<OrderDto?> GetOrderByIdAsync(int id);
    Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(int userId);
    Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
    Task<bool> UpdateOrderStatusAsync(int id, string status);
    Task<bool> DeleteOrderAsync(int id);
}
