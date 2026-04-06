namespace Whycespace.Projections.Business.Localization.Locale;

public sealed record LocaleView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
