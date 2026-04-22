using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.TrustSystem.Access.Session;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Observability;
using Whycespace.Shared.Contracts.Trust.Access.Session;

namespace Whycespace.Engines.T2E.Trust.Access.Session;

public sealed class OpenSessionHandler : IEngine
{
    private readonly ITrustMetrics _metrics;

    public OpenSessionHandler(ITrustMetrics metrics) => _metrics = metrics;

    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not OpenSessionCommand cmd)
            return Task.CompletedTask;

        var aggregate = SessionAggregate.Open(
            new SessionId(cmd.SessionId),
            new SessionDescriptor(cmd.IdentityReference, cmd.SessionContext),
            new Timestamp(cmd.OpenedAt));

        context.EmitEvents(aggregate.DomainEvents);
        _metrics.RecordSessionOpened(cmd.SessionContext);
        return Task.CompletedTask;
    }
}
