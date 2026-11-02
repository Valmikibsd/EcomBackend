using System.Data;
using Dapper;
using Order.Application.Interfaces;
using Order.Domain.Entities;
using SharedKernel.Data;

namespace Order.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public OrderRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> CreateAsync(OrderEntity order)
    {
        using var conn = _connectionFactory.CreateConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();
        try
        {
            // Insert Order header via direct table query
            const string insertOrder = @"
                INSERT INTO [Orders] ([UserId], [OrderDate], [TotalAmount], [Status], [ShippingAddress], [PaymentMethod], [PaymentStatus], [TransactionId])
                VALUES (@UserId, @OrderDate, @TotalAmount, @Status, @ShippingAddress, @PaymentMethod, @PaymentStatus, @TransactionId);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var orderId = await conn.ExecuteScalarAsync<int>(insertOrder, new
            {
                order.UserId,
                OrderDate = DateTime.UtcNow,
                order.TotalAmount,
                order.Status,
                order.ShippingAddress,
                order.PaymentMethod,
                order.PaymentStatus,
                order.TransactionId
            }, transaction);

            // Insert Order Items
            const string insertItem = @"
                INSERT INTO [OrderItems] ([OrderId], [ProductId], [ProductName], [UnitPrice], [Quantity], [TotalPrice])
                VALUES (@OrderId, @ProductId, @ProductName, @UnitPrice, @Quantity, @TotalPrice);";

            foreach (var item in order.Items)
            {
                await conn.ExecuteAsync(insertItem, new
                {
                    OrderId = orderId,
                    item.ProductId,
                    item.ProductName,
                    item.UnitPrice,
                    item.Quantity,
                    item.TotalPrice
                }, transaction);
            }

            transaction.Commit();
            return orderId;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<OrderEntity?> GetByIdAsync(int id)
    {
        using var conn = _connectionFactory.CreateConnection();
        try
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            var lookup = new Dictionary<int, OrderEntity>();
            await conn.QueryAsync<OrderEntity, OrderItemEntity, OrderEntity>(
                "USP_Order_GetById",
                (order, item) =>
                {
                    if (!lookup.TryGetValue(order.Id, out var existing))
                    {
                        existing = order;
                        lookup[order.Id] = existing;
                    }
                    if (item != null && item.Id > 0)
                        existing.Items.Add(item);
                    return existing;
                },
                parameters,
                splitOn: "OrderItemId",
                commandType: CommandType.StoredProcedure
            );
            return lookup.Values.FirstOrDefault();
        }
        catch
        {
            // Direct table query fallback
            const string sql = @"
                SELECT o.[Id] AS OrderId, o.[UserId], o.[OrderDate], o.[TotalAmount], o.[Status], o.[ShippingAddress],
                       o.[PaymentMethod], o.[PaymentStatus], o.[TransactionId],
                       oi.[Id] AS OrderItemId, oi.[ProductId], oi.[ProductName], oi.[UnitPrice], oi.[Quantity], oi.[TotalPrice]
                FROM [Orders] o
                LEFT JOIN [OrderItems] oi ON o.[Id] = oi.[OrderId]
                WHERE o.[Id] = @Id";

            var rows = await conn.QueryAsync<dynamic>(sql, new { Id = id });
            OrderEntity? result = null;
            foreach (var row in rows)
            {
                if (result == null)
                {
                    result = new OrderEntity
                    {
                        Id = (int)row.OrderId,
                        UserId = (int)row.UserId,
                        OrderDate = (DateTime)row.OrderDate,
                        TotalAmount = (decimal)row.TotalAmount,
                        Status = (string)(row.Status ?? "Paid"),
                        ShippingAddress = (string?)row.ShippingAddress,
                        PaymentMethod = (string)(row.PaymentMethod ?? "Credit Card (Mock)"),
                        PaymentStatus = (string)(row.PaymentStatus ?? "Success"),
                        TransactionId = (string?)row.TransactionId,
                        Items = new List<OrderItemEntity>()
                    };
                }
                if (row.OrderItemId != null && (int)row.OrderItemId > 0)
                {
                    result.Items.Add(new OrderItemEntity
                    {
                        Id = (int)row.OrderItemId,
                        OrderId = (int)row.OrderId,
                        ProductId = (int)row.ProductId,
                        ProductName = (string)row.ProductName,
                        UnitPrice = (decimal)row.UnitPrice,
                        Quantity = (int)row.Quantity,
                        TotalPrice = (decimal)row.TotalPrice
                    });
                }
            }
            return result;
        }
    }

    public async Task<IEnumerable<OrderEntity>> GetByUserIdAsync(int userId)
    {
        using var conn = _connectionFactory.CreateConnection();
        try
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);

            var lookup = new Dictionary<int, OrderEntity>();
            await conn.QueryAsync<OrderEntity, OrderItemEntity, OrderEntity>(
                "USP_Order_GetByUserId",
                (order, item) =>
                {
                    if (!lookup.TryGetValue(order.Id, out var existing))
                    {
                        existing = order;
                        lookup[order.Id] = existing;
                    }
                    if (item != null && item.Id > 0)
                        existing.Items.Add(item);
                    return existing;
                },
                parameters,
                splitOn: "OrderItemId",
                commandType: CommandType.StoredProcedure
            );
            return lookup.Values;
        }
        catch
        {
            const string sql = @"
                SELECT o.[Id] AS OrderId, o.[UserId], o.[OrderDate], o.[TotalAmount], o.[Status], o.[ShippingAddress],
                       o.[PaymentMethod], o.[PaymentStatus], o.[TransactionId],
                       oi.[Id] AS OrderItemId, oi.[ProductId], oi.[ProductName], oi.[UnitPrice], oi.[Quantity], oi.[TotalPrice]
                FROM [Orders] o
                LEFT JOIN [OrderItems] oi ON o.[Id] = oi.[OrderId]
                WHERE o.[UserId] = @UserId
                ORDER BY o.[OrderDate] DESC";

            var rows = await conn.QueryAsync<dynamic>(sql, new { UserId = userId });
            var lookup = new Dictionary<int, OrderEntity>();
            foreach (var row in rows)
            {
                int orderId = (int)row.OrderId;
                if (!lookup.TryGetValue(orderId, out var order))
                {
                    order = new OrderEntity
                    {
                        Id = orderId,
                        UserId = (int)row.UserId,
                        OrderDate = (DateTime)row.OrderDate,
                        TotalAmount = (decimal)row.TotalAmount,
                        Status = (string)(row.Status ?? "Paid"),
                        ShippingAddress = (string?)row.ShippingAddress,
                        PaymentMethod = (string)(row.PaymentMethod ?? "Credit Card (Mock)"),
                        PaymentStatus = (string)(row.PaymentStatus ?? "Success"),
                        TransactionId = (string?)row.TransactionId,
                        Items = new List<OrderItemEntity>()
                    };
                    lookup[orderId] = order;
                }
                if (row.OrderItemId != null && (int)row.OrderItemId > 0)
                {
                    order.Items.Add(new OrderItemEntity
                    {
                        Id = (int)row.OrderItemId,
                        OrderId = orderId,
                        ProductId = (int)row.ProductId,
                        ProductName = (string)row.ProductName,
                        UnitPrice = (decimal)row.UnitPrice,
                        Quantity = (int)row.Quantity,
                        TotalPrice = (decimal)row.TotalPrice
                    });
                }
            }
            return lookup.Values;
        }
    }

    public async Task<IEnumerable<OrderEntity>> GetAllAsync()
    {
        using var conn = _connectionFactory.CreateConnection();
        try
        {
            var lookup = new Dictionary<int, OrderEntity>();
            await conn.QueryAsync<OrderEntity, OrderItemEntity, OrderEntity>(
                "USP_Order_GetAll",
                (order, item) =>
                {
                    if (!lookup.TryGetValue(order.Id, out var existing))
                    {
                        existing = order;
                        lookup[order.Id] = existing;
                    }
                    if (item != null && item.Id > 0)
                        existing.Items.Add(item);
                    return existing;
                },
                splitOn: "OrderItemId",
                commandType: CommandType.StoredProcedure
            );
            return lookup.Values;
        }
        catch
        {
            const string sql = @"
                SELECT o.[Id] AS OrderId, o.[UserId], o.[OrderDate], o.[TotalAmount], o.[Status], o.[ShippingAddress],
                       o.[PaymentMethod], o.[PaymentStatus], o.[TransactionId],
                       oi.[Id] AS OrderItemId, oi.[ProductId], oi.[ProductName], oi.[UnitPrice], oi.[Quantity], oi.[TotalPrice]
                FROM [Orders] o
                LEFT JOIN [OrderItems] oi ON o.[Id] = oi.[OrderId]
                ORDER BY o.[OrderDate] DESC";

            var rows = await conn.QueryAsync<dynamic>(sql);
            var lookup = new Dictionary<int, OrderEntity>();
            foreach (var row in rows)
            {
                int orderId = (int)row.OrderId;
                if (!lookup.TryGetValue(orderId, out var order))
                {
                    order = new OrderEntity
                    {
                        Id = orderId,
                        UserId = (int)row.UserId,
                        OrderDate = (DateTime)row.OrderDate,
                        TotalAmount = (decimal)row.TotalAmount,
                        Status = (string)(row.Status ?? "Paid"),
                        ShippingAddress = (string?)row.ShippingAddress,
                        PaymentMethod = (string)(row.PaymentMethod ?? "Credit Card (Mock)"),
                        PaymentStatus = (string)(row.PaymentStatus ?? "Success"),
                        TransactionId = (string?)row.TransactionId,
                        Items = new List<OrderItemEntity>()
                    };
                    lookup[orderId] = order;
                }
                if (row.OrderItemId != null && (int)row.OrderItemId > 0)
                {
                    order.Items.Add(new OrderItemEntity
                    {
                        Id = (int)row.OrderItemId,
                        OrderId = orderId,
                        ProductId = (int)row.ProductId,
                        ProductName = (string)row.ProductName,
                        UnitPrice = (decimal)row.UnitPrice,
                        Quantity = (int)row.Quantity,
                        TotalPrice = (decimal)row.TotalPrice
                    });
                }
            }
            return lookup.Values;
        }
    }

    public async Task<bool> UpdateStatusAsync(int id, string status)
    {
        using var conn = _connectionFactory.CreateConnection();
        try
        {
            var parameters = new DynamicParameters();
            parameters.Add("@OrderId", id);
            parameters.Add("@Status", status);

            var rows = await conn.ExecuteAsync(
                "USP_Order_UpdateStatus",
                parameters,
                commandType: CommandType.StoredProcedure
            );
            return rows > 0;
        }
        catch
        {
            const string sql = "UPDATE [Orders] SET [Status] = @Status WHERE [Id] = @Id";
            var rows = await conn.ExecuteAsync(sql, new { Id = id, Status = status });
            return rows > 0;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = _connectionFactory.CreateConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();
        try
        {
            const string sql = @"
                DELETE FROM [OrderItems] WHERE [OrderId] = @Id;
                DELETE FROM [Orders] WHERE [Id] = @Id;";
            var rows = await conn.ExecuteAsync(sql, new { Id = id }, transaction);
            transaction.Commit();
            return rows > 0;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
