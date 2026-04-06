namespace Whycespace.Projections.Business.Notification.Template;

public sealed record TemplateView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
