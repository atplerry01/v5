namespace Whycespace.Engines.T2E.Decision.Risk.Exception;

public class ExceptionEngine
{
    private readonly ExceptionPolicyAdapter _policy;

    public ExceptionEngine(ExceptionPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<ExceptionResult> ExecuteAsync(ExceptionCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new ExceptionResult(true, "Executed");
    }
}
