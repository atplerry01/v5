namespace Whycespace.Engines.T2E.Business.Resource.AssetResource;

public class AssetResourceEngine
{
    private readonly AssetResourcePolicyAdapter _policy;

    public AssetResourceEngine(AssetResourcePolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<AssetResourceResult> ExecuteAsync(AssetResourceCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new AssetResourceResult(true, "Executed");
    }
}
