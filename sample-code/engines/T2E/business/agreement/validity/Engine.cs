namespace Whycespace.Engines.T2E.Business.Agreement.Validity;

public class ValidityEngine
{
    private readonly ValidityPolicyAdapter _policy;

    public ValidityEngine(ValidityPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<ValidityResult> ExecuteAsync(ValidityCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new ValidityResult(true, "Executed");
    }
}
