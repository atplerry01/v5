namespace Whycespace.Platform.Api.Business.Resource.Equipment;

public sealed record EquipmentRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record EquipmentResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
