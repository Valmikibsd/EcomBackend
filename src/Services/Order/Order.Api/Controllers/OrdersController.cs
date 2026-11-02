using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Order.Application.DTOs;
using Order.Application.Interfaces;

namespace Order.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
    {
        try
        {
            if (dto.Items == null || dto.Items.Count == 0)
                return BadRequest(new { message = "Order must contain at least one item." });

            var order = await _orderService.CreateOrderAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error creating order.", detail = ex.Message });
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound(new { message = $"Order {id} not found." });

            return Ok(order);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving order.", detail = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving all orders.", detail = ex.Message });
        }
    }

    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetByUserId(int userId)
    {
        try
        {
            var orders = await _orderService.GetOrdersByUserIdAsync(userId);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving orders.", detail = ex.Message });
        }
    }

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusDto body)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(body?.Status))
                return BadRequest(new { message = "Status cannot be empty." });

            var success = await _orderService.UpdateOrderStatusAsync(id, body.Status);
            if (!success)
                return NotFound(new { message = $"Order {id} not found." });

            return Ok(new { message = $"Order {id} status updated to {body.Status}." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error updating order status.", detail = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var success = await _orderService.DeleteOrderAsync(id);
            if (!success)
                return NotFound(new { message = $"Order {id} not found." });

            return Ok(new { message = $"Order {id} deleted successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error deleting order.", detail = ex.Message });
        }
    }
}

public class UpdateOrderStatusDto
{
    public string Status { get; set; } = string.Empty;
}
