namespace Whycespace.Engines.T2E.Business.Integration.Gateway;

public class GatewayEngine
{
    private readonly GatewayPolicyAdapter _policy;

    public GatewayEngine(GatewayPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<GatewayResult> ExecuteAsync(GatewayCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new GatewayResult(true, "Executed");
    }
}
