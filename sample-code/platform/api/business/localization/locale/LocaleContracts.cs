namespace Whycespace.Platform.Api.Business.Localization.Locale;

public sealed record LocaleRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record LocaleResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
