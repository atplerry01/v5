namespace Whycespace.Domain.BusinessSystem.Integration.Webhook;

public readonly record struct WebhookEndpointId
{
    public Guid Value { get; }

    public WebhookEndpointId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("WebhookEndpointId value must not be empty.", nameof(value));
        Value = value;
    }
}
