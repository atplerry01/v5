using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Agreement.Signature;

public sealed class SignaturePolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
