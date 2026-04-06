using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Document.Record;

public sealed class RecordPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
