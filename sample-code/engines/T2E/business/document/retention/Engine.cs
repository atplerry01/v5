namespace Whycespace.Engines.T2E.Business.Document.Retention;

public class RetentionEngine
{
    private readonly RetentionPolicyAdapter _policy;

    public RetentionEngine(RetentionPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<RetentionResult> ExecuteAsync(RetentionCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new RetentionResult(true, "Executed");
    }
}
