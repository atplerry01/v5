using Whycespace.Domain.TrustSystem.Identity.Registry;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Observability;
using Whycespace.Shared.Contracts.Trust.Identity.Registry;

namespace Whycespace.Engines.T2E.Trust.Identity.Registry;

public sealed class ActivateRegistrationHandler : IEngine
{
    private readonly ITrustMetrics _metrics;

    public ActivateRegistrationHandler(ITrustMetrics metrics) => _metrics = metrics;

    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ActivateRegistrationCommand)
            return;

        var aggregate = (RegistryAggregate)await context.LoadAggregateAsync(typeof(RegistryAggregate));
        aggregate.Activate();
        context.EmitEvents(aggregate.DomainEvents);
        _metrics.RecordRegistrationActivated(aggregate.Descriptor.RegistrationType);
    }
}
