namespace Whycespace.Domain.BusinessSystem.Integration.Webhook;

public readonly record struct WebhookId
{
    public Guid Value { get; }

    public WebhookId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("WebhookId value must not be empty.", nameof(value));
        Value = value;
    }
}
