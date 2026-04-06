namespace Whycespace.Engines.T2E.Business.Integration.Handshake;

public class HandshakeEngine
{
    private readonly HandshakePolicyAdapter _policy;

    public HandshakeEngine(HandshakePolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<HandshakeResult> ExecuteAsync(HandshakeCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new HandshakeResult(true, "Executed");
    }
}
