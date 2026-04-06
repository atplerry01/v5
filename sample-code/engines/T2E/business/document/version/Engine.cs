namespace Whycespace.Engines.T2E.Business.Document.Version;

public class VersionEngine
{
    private readonly VersionPolicyAdapter _policy;

    public VersionEngine(VersionPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<VersionResult> ExecuteAsync(VersionCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new VersionResult(true, "Executed");
    }
}
