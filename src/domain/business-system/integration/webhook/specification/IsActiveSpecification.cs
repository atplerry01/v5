namespace Whycespace.Domain.BusinessSystem.Integration.Webhook;

public sealed class IsActiveSpecification
{
    public bool IsSatisfiedBy(WebhookStatus status)
    {
        return status == WebhookStatus.Active;
    }
}
