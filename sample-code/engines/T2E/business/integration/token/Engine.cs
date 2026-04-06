namespace Whycespace.Engines.T2E.Business.Integration.Token;

public class TokenEngine
{
    private readonly TokenPolicyAdapter _policy;

    public TokenEngine(TokenPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<TokenResult> ExecuteAsync(TokenCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new TokenResult(true, "Executed");
    }
}
