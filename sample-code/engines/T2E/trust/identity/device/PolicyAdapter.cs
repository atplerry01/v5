using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Trust.Identity.Device;

public sealed class DevicePolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
