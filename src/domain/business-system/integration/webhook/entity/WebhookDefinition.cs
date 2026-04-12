namespace Whycespace.Domain.BusinessSystem.Integration.Webhook;

public sealed class WebhookDefinition
{
    public WebhookEndpointId EndpointId { get; }
    public string WebhookName { get; }
    public string TargetUri { get; }

    public WebhookDefinition(WebhookEndpointId endpointId, string webhookName, string targetUri)
    {
        if (endpointId == default)
            throw new ArgumentException("EndpointId must not be empty.", nameof(endpointId));

        if (string.IsNullOrWhiteSpace(webhookName))
            throw new ArgumentException("WebhookName must not be empty.", nameof(webhookName));

        if (string.IsNullOrWhiteSpace(targetUri))
            throw new ArgumentException("TargetUri must not be empty.", nameof(targetUri));

        EndpointId = endpointId;
        WebhookName = webhookName;
        TargetUri = targetUri;
    }
}
