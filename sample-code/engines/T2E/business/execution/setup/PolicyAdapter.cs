using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Execution.Setup;

public sealed class SetupPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
