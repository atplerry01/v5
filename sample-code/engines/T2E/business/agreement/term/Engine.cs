namespace Whycespace.Engines.T2E.Business.Agreement.Term;

public class TermEngine
{
    private readonly TermPolicyAdapter _policy;

    public TermEngine(TermPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<TermResult> ExecuteAsync(TermCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new TermResult(true, "Executed");
    }
}
