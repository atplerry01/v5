using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Document.Version;

public sealed class VersionPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
