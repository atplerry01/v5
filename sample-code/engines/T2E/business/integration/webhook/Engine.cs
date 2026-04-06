namespace Whycespace.Engines.T2E.Business.Integration.Webhook;

public class WebhookEngine
{
    private readonly WebhookPolicyAdapter _policy;

    public WebhookEngine(WebhookPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<WebhookResult> ExecuteAsync(WebhookCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new WebhookResult(true, "Executed");
    }
}
