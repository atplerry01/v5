namespace Whycespace.Engines.T2E.Decision.Risk.Control;

public class ControlEngine
{
    private readonly ControlPolicyAdapter _policy;

    public ControlEngine(ControlPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<ControlResult> ExecuteAsync(ControlCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new ControlResult(true, "Executed");
    }
}
