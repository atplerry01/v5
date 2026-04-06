using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Document.Retention;

public sealed class RetentionPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
