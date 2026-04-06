using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Integration.Synchronization;

public sealed class SynchronizationPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
