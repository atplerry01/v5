namespace Whycespace.Engines.T2E.Business.Integration.Webhook;

public record WebhookCommand(
    string Action,
    string EntityId,
    object Payload
);
