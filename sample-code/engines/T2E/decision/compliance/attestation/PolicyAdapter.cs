using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Decision.Compliance.Attestation;

public sealed class AttestationPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
