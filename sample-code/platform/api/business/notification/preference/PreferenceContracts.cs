namespace Whycespace.Platform.Api.Business.Notification.Preference;

public sealed record PreferenceRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record PreferenceResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
