namespace Whycespace.Projections.Business.Integration.Webhook;

public sealed record WebhookView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
