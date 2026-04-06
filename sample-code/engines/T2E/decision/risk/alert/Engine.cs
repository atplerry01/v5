namespace Whycespace.Engines.T2E.Decision.Risk.Alert;

public class AlertEngine
{
    private readonly AlertPolicyAdapter _policy;

    public AlertEngine(AlertPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<AlertResult> ExecuteAsync(AlertCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new AlertResult(true, "Executed");
    }
}
