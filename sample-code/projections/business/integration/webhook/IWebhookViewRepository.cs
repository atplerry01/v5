namespace Whycespace.Projections.Business.Integration.Webhook;

public interface IWebhookViewRepository
{
    Task SaveAsync(WebhookReadModel model, CancellationToken ct = default);
    Task<WebhookReadModel?> GetAsync(string id, CancellationToken ct = default);
}
