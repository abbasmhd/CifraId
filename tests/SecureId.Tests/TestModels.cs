using System.Text.Json.Serialization;
using SecureId.Attributes;

namespace SecureId.Tests;

public class TestDto
{
    [SecureId]
    public int Id { get; set; }

    public string? Name { get; set; }

    [SecureId]
    public int? ParentId { get; set; }

    [JsonIgnore]
    public int Secret { get; set; }

    [SecureId("customId")]
    public int CustomNamedId { get; set; }
}

public class TestDtoWithEnum
{
    [SecureId]
    public TestStatus Status { get; set; }

    [SecureId]
    public TestStatus? NullableStatus { get; set; }
}

public enum TestStatus
{
    Active = 1,
    Inactive = 2,
    Archived = 3,
}

public class QueryModel
{
    [SecureId]
    public int Id { get; set; }

    public string? Name { get; set; }
}

public class QueryModelWithNullable
{
    [SecureId]
    public int? OrderId { get; set; }

    [SecureId]
    public TestStatus? Status { get; set; }
}
