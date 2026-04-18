using Whycespace.Domain.EconomicSystem.Risk.Exposure;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Risk.Exposure;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Risk.Exposure;

/// <summary>
/// Phase 6 T6.5 — command handler for the risk → enforcement loop entry
/// point. Loads the exposure aggregate, asks it to evaluate the supplied
/// threshold, and (if breached) emits <see cref="ExposureBreachedEvent"/>
/// for downstream consumption by
/// <c>RiskExposureEnforcementHandler</c>.
///
/// No direct enforcement dispatch here — cross-aggregate dispatch belongs
/// to a runtime integration handler, not a T2E aggregate handler.
/// </summary>
public sealed class DetectRiskExposureBreachHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DetectRiskExposureBreachCommand cmd)
            return;

        var aggregate = (ExposureAggregate)await context.LoadAggregateAsync(typeof(ExposureAggregate));
        aggregate.DetectBreach(new Amount(cmd.Threshold), new Timestamp(cmd.DetectedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
