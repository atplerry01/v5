namespace Whycespace.Platform.Api.Business.Localization.Translation;

public sealed record TranslationRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record TranslationResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
