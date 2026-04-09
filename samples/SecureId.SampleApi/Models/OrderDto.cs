using SecureId.Attributes;

namespace SecureId.SampleApi.Models;

public class OrderResponseDto
{
    [SecureId]
    public int OrderId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    [SecureId]
    public OrderStatus Status { get; set; }

    public decimal TotalAmount { get; set; }

    [SecureId]
    public int? AssignedAgentId { get; set; }
}

public class OrderQueryDto
{
    [SecureId]
    public int OrderId { get; set; }

    [SecureId]
    public OrderStatus? Status { get; set; }
}

public enum OrderStatus
{
    Pending = 0,
    Processing = 1,
    Shipped = 2,
    Delivered = 3,
    Cancelled = 4,
}
