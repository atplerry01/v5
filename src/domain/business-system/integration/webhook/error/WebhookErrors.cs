namespace Whycespace.Domain.BusinessSystem.Integration.Webhook;

public static class WebhookErrors
{
    public static WebhookDomainException MissingId()
        => new("WebhookId is required and must not be empty.");

    public static WebhookDomainException MissingDefinition()
        => new("WebhookDefinition is required and must not be null.");

    public static WebhookDomainException InvalidStateTransition(WebhookStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static WebhookDomainException AlreadyActive(WebhookId id)
        => new($"Webhook '{id.Value}' is already active.");

    public static WebhookDomainException AlreadyDisabled(WebhookId id)
        => new($"Webhook '{id.Value}' is already disabled.");
}

public sealed class WebhookDomainException : Exception
{
    public WebhookDomainException(string message) : base(message) { }
}
