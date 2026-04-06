namespace Whycespace.Projections.Business.Integration.Callback;

public sealed record CallbackView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
