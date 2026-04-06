namespace Whycespace.Engines.T2E.Decision.Governance.Charter;

public class CharterEngine
{
    private readonly CharterPolicyAdapter _policy;

    public CharterEngine(CharterPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<CharterResult> ExecuteAsync(CharterCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new CharterResult(true, "Executed");
    }
}
