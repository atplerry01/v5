namespace Whycespace.Engines.T2E.Business.Integration.Client;

public class ClientEngine
{
    private readonly ClientPolicyAdapter _policy;

    public ClientEngine(ClientPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<ClientResult> ExecuteAsync(ClientCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new ClientResult(true, "Executed");
    }
}
