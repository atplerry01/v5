using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Notification.Channel;

public sealed class ChannelPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
