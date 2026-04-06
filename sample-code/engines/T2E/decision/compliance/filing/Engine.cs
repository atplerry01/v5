namespace Whycespace.Engines.T2E.Decision.Compliance.Filing;

public class FilingEngine
{
    private readonly FilingPolicyAdapter _policy;

    public FilingEngine(FilingPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<FilingResult> ExecuteAsync(FilingCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new FilingResult(true, "Executed");
    }
}
