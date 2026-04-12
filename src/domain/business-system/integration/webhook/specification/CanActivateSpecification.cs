namespace Whycespace.Domain.BusinessSystem.Integration.Webhook;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(WebhookStatus status)
    {
        return status is WebhookStatus.Defined or WebhookStatus.Disabled;
    }
}
