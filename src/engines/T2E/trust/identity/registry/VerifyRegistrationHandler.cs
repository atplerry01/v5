using Whycespace.Domain.TrustSystem.Identity.Registry;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Observability;
using Whycespace.Shared.Contracts.Trust.Identity.Registry;

namespace Whycespace.Engines.T2E.Trust.Identity.Registry;

public sealed class VerifyRegistrationHandler : IEngine
{
    private readonly ITrustMetrics _metrics;

    public VerifyRegistrationHandler(ITrustMetrics metrics) => _metrics = metrics;

    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not VerifyRegistrationCommand)
            return;

        var aggregate = (RegistryAggregate)await context.LoadAggregateAsync(typeof(RegistryAggregate));
        aggregate.Verify();
        context.EmitEvents(aggregate.DomainEvents);
        _metrics.RecordRegistrationVerified(aggregate.Descriptor.RegistrationType);
    }
}
