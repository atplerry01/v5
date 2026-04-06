using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Document.Evidence;

public sealed class EvidencePolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
