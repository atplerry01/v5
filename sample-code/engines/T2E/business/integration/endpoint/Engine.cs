namespace Whycespace.Engines.T2E.Business.Integration.Endpoint;

public class EndpointEngine
{
    private readonly EndpointPolicyAdapter _policy;

    public EndpointEngine(EndpointPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<EndpointResult> ExecuteAsync(EndpointCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new EndpointResult(true, "Executed");
    }
}
