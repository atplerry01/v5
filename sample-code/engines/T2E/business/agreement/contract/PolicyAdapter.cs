using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Agreement.Contract;

public sealed class ContractPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
