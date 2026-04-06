namespace Whycespace.Projections.Business.Localization.Translation;

public sealed record TranslationView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
