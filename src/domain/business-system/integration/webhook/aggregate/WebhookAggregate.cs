namespace Whycespace.Domain.BusinessSystem.Integration.Webhook;

public sealed class WebhookAggregate
{
    public static WebhookAggregate Create()
    {
        var aggregate = new WebhookAggregate();
        aggregate.ValidateBeforeChange();
        aggregate.EnsureInvariants();
        // POLICY HOOK (to be enforced by runtime)
        return aggregate;
    }

    private void EnsureInvariants()
    {
        // Domain invariant checks enforced BEFORE any event is raised
    }

    private void ValidateBeforeChange()
    {
        // Pre-change validation gate
    }
}
