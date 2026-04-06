namespace Whycespace.Engines.T2E.Business.Integration.Transformation;

public class TransformationEngine
{
    private readonly TransformationPolicyAdapter _policy;

    public TransformationEngine(TransformationPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<TransformationResult> ExecuteAsync(TransformationCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new TransformationResult(true, "Executed");
    }
}
