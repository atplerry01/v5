namespace Whycespace.Engines.T2E.Business.Inventory.Writeoff;

public class WriteoffEngine
{
    private readonly WriteoffPolicyAdapter _policy;

    public WriteoffEngine(WriteoffPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<WriteoffResult> ExecuteAsync(WriteoffCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new WriteoffResult(true, "Executed");
    }
}
