using Whycespace.Domain.EconomicSystem.Enforcement.Lock;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Enforcement.Lock;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Enforcement.Lock;

public sealed class LockSystemHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not LockSystemCommand cmd)
            return Task.CompletedTask;

        if (!Enum.TryParse<LockScope>(cmd.Scope, ignoreCase: true, out var scope))
            throw new InvalidOperationException($"Unknown lock scope: '{cmd.Scope}'.");

        var aggregate = LockAggregate.Lock(
            new LockId(cmd.LockId),
            new SubjectId(cmd.SubjectId),
            scope,
            new Reason(cmd.Reason),
            new Timestamp(cmd.LockedAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
