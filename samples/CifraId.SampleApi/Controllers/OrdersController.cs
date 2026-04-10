using Microsoft.AspNetCore.Mvc;
using CifraId.SampleApi.Models;
using CifraId.Services;

namespace CifraId.SampleApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly ICifraIdService _cifraId;

    public OrdersController(ICifraIdService cifraId) => _cifraId = cifraId;

    /// <summary>
    /// Returns a list of orders with encoded IDs and enum values.
    /// </summary>
    [HttpGet]
    public ActionResult<OrderResponseDto[]> GetOrders()
    {
        var orders = new[]
        {
            new OrderResponseDto
            {
                OrderId = 1,
                CustomerName = "Alice",
                Status = OrderStatus.Processing,
                TotalAmount = 99.99m,
            },
            new OrderResponseDto
            {
                OrderId = 2,
                CustomerName = "Bob",
                Status = OrderStatus.Shipped,
                TotalAmount = 149.50m,
                AssignedAgentId = 10,
            },
            new OrderResponseDto
            {
                OrderId = 3,
                CustomerName = "Charlie",
                Status = OrderStatus.Delivered,
                TotalAmount = 299.00m,
            },
        };

        return Ok(orders);
    }

    /// <summary>
    /// Retrieves a single order by its encoded ID (passed as a route segment).
    /// </summary>
    [HttpGet("{encodedId}")]
    public ActionResult<OrderResponseDto> GetOrder(string encodedId)
    {
        var orderId = _cifraId.DecodeId(encodedId);
        if (orderId is null)
        {
            return BadRequest("Invalid order ID.");
        }

        return Ok(new OrderResponseDto
        {
            OrderId = orderId.Value,
            CustomerName = "Alice",
            Status = OrderStatus.Processing,
            TotalAmount = 99.99m,
        });
    }

    /// <summary>
    /// Demonstrates query-string model binding with CifraId. Pass encoded orderId and/or status via query string.
    /// </summary>
    [HttpGet("search")]
    public ActionResult<OrderResponseDto[]> SearchOrders([FromQuery] OrderQueryDto query)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        return Ok(new[]
        {
            new OrderResponseDto
            {
                OrderId = query.OrderId,
                CustomerName = "Found Customer",
                Status = query.Status ?? OrderStatus.Pending,
                TotalAmount = 50.00m,
            },
        });
    }

    /// <summary>
    /// Shows manual encode/decode usage via ICifraIdService.
    /// </summary>
    [HttpGet("manual")]
    public ActionResult ManualExample()
    {
        var encoded = _cifraId.EncodeId(42);
        var decoded = _cifraId.DecodeId(encoded);

        return Ok(new { Original = 42, Encoded = encoded, Decoded = decoded });
    }
}
