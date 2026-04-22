using Whycespace.Domain.TrustSystem.Identity.Verification;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Observability;
using Whycespace.Shared.Contracts.Trust.Identity.Verification;

namespace Whycespace.Engines.T2E.Trust.Identity.Verification;

public sealed class InitiateVerificationHandler : IEngine
{
    private readonly ITrustMetrics _metrics;

    public InitiateVerificationHandler(ITrustMetrics metrics) => _metrics = metrics;

    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not InitiateVerificationCommand cmd)
            return Task.CompletedTask;

        var aggregate = VerificationAggregate.Initiate(
            new VerificationId(cmd.VerificationId),
            new VerificationSubject(cmd.IdentityReference, cmd.ClaimType));

        context.EmitEvents(aggregate.DomainEvents);
        _metrics.RecordVerificationInitiated(cmd.ClaimType);
        return Task.CompletedTask;
    }
}
