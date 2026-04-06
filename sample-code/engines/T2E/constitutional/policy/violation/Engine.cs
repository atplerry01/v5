namespace Whycespace.Engines.T2E.Constitutional.Policy.Violation;

public class ViolationEngine
{
    private readonly ViolationPolicyAdapter _policy;

    public ViolationEngine(ViolationPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<ViolationResult> ExecuteAsync(ViolationCommand command)
    {
        await _policy.EnforceAsync(command);

        // Domain logic — emit events only (no persistence)

        return new ViolationResult(true, "Executed");
    }
}
