using Order.Application.DTOs;
using Order.Application.Interfaces;
using Order.Domain.Entities;

namespace Order.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;

    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto)
    {
        if (dto.Items == null || dto.Items.Count == 0)
            throw new ArgumentException("Order must have at least one item.");

        var items = dto.Items.Select(i => new OrderItemEntity
        {
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            UnitPrice = i.UnitPrice,
            Quantity = i.Quantity,
            TotalPrice = i.UnitPrice * i.Quantity
        }).ToList();

        // Generate a mock payment transaction ID
        var transactionId = "TXN-" + Guid.NewGuid().ToString("N")[..10].ToUpper();

        var order = new OrderEntity
        {
            UserId = dto.UserId,
            ShippingAddress = dto.ShippingAddress,
            TotalAmount = items.Sum(i => i.TotalPrice),
            OrderDate = DateTime.UtcNow,
            Status = "Paid",
            PaymentMethod = string.IsNullOrWhiteSpace(dto.PaymentMethod) ? "Credit Card (Mock)" : dto.PaymentMethod,
            PaymentStatus = "Success",
            TransactionId = transactionId,
            Items = items
        };

        var id = await _orderRepository.CreateAsync(order);
        order.Id = id;
        return MapToDto(order);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        return order == null ? null : MapToDto(order);
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(int userId)
    {
        var orders = await _orderRepository.GetByUserIdAsync(userId);
        return orders.Select(MapToDto);
    }

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
    {
        var orders = await _orderRepository.GetAllAsync();
        return orders.Select(MapToDto);
    }

    public async Task<bool> UpdateOrderStatusAsync(int id, string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Status is required.");

        return await _orderRepository.UpdateStatusAsync(id, status.Trim());
    }

    public async Task<bool> DeleteOrderAsync(int id)
    {
        return await _orderRepository.DeleteAsync(id);
    }

    private static OrderDto MapToDto(OrderEntity o) => new()
    {
        Id = o.Id,
        UserId = o.UserId,
        OrderDate = o.OrderDate.ToString("o"),
        TotalAmount = o.TotalAmount,
        Status = o.Status,
        ShippingAddress = o.ShippingAddress,
        PaymentMethod = o.PaymentMethod,
        PaymentStatus = o.PaymentStatus,
        TransactionId = o.TransactionId,
        Items = o.Items.Select(i => new OrderItemDto
        {
            Id = i.Id,
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            UnitPrice = i.UnitPrice,
            Quantity = i.Quantity,
            TotalPrice = i.TotalPrice
        }).ToList()
    };
}
