namespace Whycespace.Engines.T2E.Business.Integration.Connector;

public class ConnectorEngine
{
    private readonly ConnectorPolicyAdapter _policy;

    public ConnectorEngine(ConnectorPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<ConnectorResult> ExecuteAsync(ConnectorCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new ConnectorResult(true, "Executed");
    }
}
