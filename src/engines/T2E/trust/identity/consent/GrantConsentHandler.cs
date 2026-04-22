using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.TrustSystem.Identity.Consent;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Observability;
using Whycespace.Shared.Contracts.Trust.Identity.Consent;

namespace Whycespace.Engines.T2E.Trust.Identity.Consent;

public sealed class GrantConsentHandler : IEngine
{
    private readonly ITrustMetrics _metrics;

    public GrantConsentHandler(ITrustMetrics metrics) => _metrics = metrics;

    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not GrantConsentCommand cmd)
            return Task.CompletedTask;

        var aggregate = ConsentAggregate.Grant(
            new ConsentId(cmd.ConsentId),
            new ConsentDescriptor(cmd.IdentityReference, cmd.ConsentScope, cmd.ConsentPurpose),
            new Timestamp(cmd.GrantedAt));

        context.EmitEvents(aggregate.DomainEvents);
        _metrics.RecordConsentGranted(cmd.ConsentScope);
        return Task.CompletedTask;
    }
}
