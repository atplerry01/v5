namespace Whycespace.Engines.T2E.Business.Agreement.Signature;

public class SignatureEngine
{
    private readonly SignaturePolicyAdapter _policy;

    public SignatureEngine(SignaturePolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<SignatureResult> ExecuteAsync(SignatureCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new SignatureResult(true, "Executed");
    }
}
