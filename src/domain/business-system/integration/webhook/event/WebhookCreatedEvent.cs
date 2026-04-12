namespace Whycespace.Domain.BusinessSystem.Integration.Webhook;

public sealed record WebhookCreatedEvent(WebhookId WebhookId, WebhookEndpointId EndpointId, string WebhookName);
