namespace Whycespace.Engines.T2E.Business.Entitlement.Quota;

public class QuotaEngine
{
    private readonly QuotaPolicyAdapter _policy;

    public QuotaEngine(QuotaPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<QuotaResult> ExecuteAsync(QuotaCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new QuotaResult(true, "Executed");
    }
}
