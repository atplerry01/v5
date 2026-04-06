namespace Whycespace.Engines.T2E.Business.Execution.Activation;

public class ActivationEngine
{
    private readonly ActivationPolicyAdapter _policy;

    public ActivationEngine(ActivationPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<ActivationResult> ExecuteAsync(ActivationCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new ActivationResult(true, "Executed");
    }
}
