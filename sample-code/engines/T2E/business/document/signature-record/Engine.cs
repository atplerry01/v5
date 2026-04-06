namespace Whycespace.Engines.T2E.Business.Document.SignatureRecord;

public class SignatureRecordEngine
{
    private readonly SignatureRecordPolicyAdapter _policy;

    public SignatureRecordEngine(SignatureRecordPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<SignatureRecordResult> ExecuteAsync(SignatureRecordCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new SignatureRecordResult(true, "Executed");
    }
}
