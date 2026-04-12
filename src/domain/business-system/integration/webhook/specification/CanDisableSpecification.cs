namespace Whycespace.Domain.BusinessSystem.Integration.Webhook;

public sealed class CanDisableSpecification
{
    public bool IsSatisfiedBy(WebhookStatus status)
    {
        return status == WebhookStatus.Active;
    }
}
