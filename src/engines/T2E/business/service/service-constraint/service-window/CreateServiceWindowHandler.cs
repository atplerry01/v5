using Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.ServiceWindow;
using Whycespace.Domain.BusinessSystem.Shared.Time;
using Whycespace.Shared.Contracts.Business.Service.ServiceConstraint.ServiceWindow;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Service.ServiceConstraint.ServiceWindow;

public sealed class CreateServiceWindowHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateServiceWindowCommand cmd)
            return Task.CompletedTask;

        var aggregate = ServiceWindowAggregate.Create(
            new ServiceWindowId(cmd.ServiceWindowId),
            new ServiceDefinitionRef(cmd.ServiceDefinitionId),
            new TimeWindow(cmd.StartsAt, cmd.EndsAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
