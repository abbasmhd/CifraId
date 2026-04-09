using System.Text.Json.Serialization;
using CifraId.Attributes;

namespace CifraId.Tests;

public class TestDto
{
    [CifraId]
    public int Id { get; set; }

    public string? Name { get; set; }

    [CifraId]
    public int? ParentId { get; set; }

    [JsonIgnore]
    public int Secret { get; set; }

    [CifraId("customId")]
    public int CustomNamedId { get; set; }
}

public class TestDtoWithEnum
{
    [CifraId]
    public TestStatus Status { get; set; }

    [CifraId]
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
    [CifraId]
    public int Id { get; set; }

    public string? Name { get; set; }
}

public class QueryModelWithNullable
{
    [CifraId]
    public int? OrderId { get; set; }

    [CifraId]
    public TestStatus? Status { get; set; }
}
