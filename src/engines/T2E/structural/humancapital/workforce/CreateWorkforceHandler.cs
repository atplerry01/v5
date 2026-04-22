using Whycespace.Domain.BusinessSystem.Workforce.Workforce;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Humancapital.Workforce;

namespace Whycespace.Engines.T2E.Structural.Humancapital.Workforce;

public sealed class CreateWorkforceHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateWorkforceCommand cmd) return Task.CompletedTask;
        var aggregate = WorkforceAggregate.Create(
            new WorkforceId(cmd.WorkforceId),
            new WorkforceDescriptor(cmd.Name, cmd.Kind));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
