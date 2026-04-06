namespace Whycespace.Engines.T2E.Constitutional.Policy.Constraint;

public class ConstraintEngine
{
    private readonly ConstraintPolicyAdapter _policy;

    public ConstraintEngine(ConstraintPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<ConstraintResult> ExecuteAsync(ConstraintCommand command)
    {
        await _policy.EnforceAsync(command);

        // Domain logic — emit events only (no persistence)

        return new ConstraintResult(true, "Executed");
    }
}
