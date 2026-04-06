namespace Whycespace.Engines.T2E.Business.Integration.Import;

public class ImportEngine
{
    private readonly ImportPolicyAdapter _policy;

    public ImportEngine(ImportPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<ImportResult> ExecuteAsync(ImportCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new ImportResult(true, "Executed");
    }
}
