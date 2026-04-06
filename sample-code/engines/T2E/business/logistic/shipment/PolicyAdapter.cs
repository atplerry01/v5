using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Logistic.Shipment;

public sealed class ShipmentPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
