namespace Whycespace.Engines.T2E.Business.Execution.Completion;

public class CompletionEngine
{
    private readonly CompletionPolicyAdapter _policy;

    public CompletionEngine(CompletionPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<CompletionResult> ExecuteAsync(CompletionCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new CompletionResult(true, "Executed");
    }
}
