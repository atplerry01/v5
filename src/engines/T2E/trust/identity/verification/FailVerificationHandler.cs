using Whycespace.Domain.TrustSystem.Identity.Verification;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Observability;
using Whycespace.Shared.Contracts.Trust.Identity.Verification;

namespace Whycespace.Engines.T2E.Trust.Identity.Verification;

public sealed class FailVerificationHandler : IEngine
{
    private readonly ITrustMetrics _metrics;

    public FailVerificationHandler(ITrustMetrics metrics) => _metrics = metrics;

    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not FailVerificationCommand)
            return;

        var aggregate = (VerificationAggregate)await context.LoadAggregateAsync(typeof(VerificationAggregate));
        aggregate.Fail();
        context.EmitEvents(aggregate.DomainEvents);
        _metrics.RecordVerificationFailed(aggregate.Subject.ClaimType);
    }
}
