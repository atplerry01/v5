using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Integration.Webhook;

public sealed class WebhookProjectionHandler
{
    public string ProjectionName => "whyce.business.integration.webhook";

    public string[] EventTypes =>
    [
        "whyce.business.integration.webhook.created",
        "whyce.business.integration.webhook.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IWebhookViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new WebhookReadModel
        {
            Id = @event.AggregateId.ToString(),
            Status = "Active",
            LastUpdated = @event.Timestamp,
            LastEventTimestamp = @event.Timestamp,
            LastEventVersion = @event.Version
        };

        await repository.SaveAsync(model, ct);
    }
}
