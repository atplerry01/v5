using Whycespace.Domain.ControlSystem.SystemReconciliation.SystemVerification;
using Whycespace.Shared.Contracts.Control.SystemReconciliation.SystemVerification;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.SystemReconciliation.SystemVerification;

public sealed class InitiateSystemVerificationHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not InitiateSystemVerificationCommand cmd)
            return Task.CompletedTask;

        var aggregate = SystemVerificationAggregate.Initiate(
            new SystemVerificationId(cmd.VerificationId.ToString("N").PadRight(64, '0')),
            cmd.TargetSystem,
            cmd.InitiatedAt);

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
