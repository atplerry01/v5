namespace Whycespace.Engines.T2E.Business.Integration.Export;

public class ExportEngine
{
    private readonly ExportPolicyAdapter _policy;

    public ExportEngine(ExportPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<ExportResult> ExecuteAsync(ExportCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new ExportResult(true, "Executed");
    }
}
