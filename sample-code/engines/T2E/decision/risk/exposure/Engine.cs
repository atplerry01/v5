namespace Whycespace.Engines.T2E.Decision.Risk.Exposure;

public class ExposureEngine
{
    private readonly ExposurePolicyAdapter _policy;

    public ExposureEngine(ExposurePolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<ExposureResult> ExecuteAsync(ExposureCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new ExposureResult(true, "Executed");
    }
}
