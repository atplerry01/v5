namespace Whycespace.Platform.Api.Business.Scheduler.Booking;

public sealed record BookingRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record BookingResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
