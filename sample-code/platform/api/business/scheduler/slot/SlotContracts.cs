namespace Whycespace.Platform.Api.Business.Scheduler.Slot;

public sealed record SlotRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record SlotResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
