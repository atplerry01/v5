namespace Whycespace.Projections.Business.Localization.Timezone;

public sealed record TimezoneView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
