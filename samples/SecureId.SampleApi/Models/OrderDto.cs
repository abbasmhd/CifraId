using CifraId.Attributes;

namespace CifraId.SampleApi.Models;

public class OrderResponseDto
{
    [CifraId]
    public int OrderId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    [CifraId]
    public OrderStatus Status { get; set; }

    public decimal TotalAmount { get; set; }

    [CifraId]
    public int? AssignedAgentId { get; set; }
}

public class OrderQueryDto
{
    [CifraId]
    public int OrderId { get; set; }

    [CifraId]
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
