namespace Whycespace.Engines.T2E.Business.Execution.Setup;

public class SetupEngine
{
    private readonly SetupPolicyAdapter _policy;

    public SetupEngine(SetupPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<SetupResult> ExecuteAsync(SetupCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new SetupResult(true, "Executed");
    }
}
