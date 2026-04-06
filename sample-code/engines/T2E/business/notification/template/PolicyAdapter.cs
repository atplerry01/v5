using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Notification.Template;

public sealed class TemplatePolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
