namespace Whycespace.Platform.Api.Business.Localization.Timezone;

public sealed record TimezoneRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record TimezoneResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
