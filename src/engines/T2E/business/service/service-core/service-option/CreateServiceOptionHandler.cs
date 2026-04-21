using Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceOption;
using Whycespace.Shared.Contracts.Business.Service.ServiceCore.ServiceOption;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Service.ServiceCore.ServiceOption;

public sealed class CreateServiceOptionHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateServiceOptionCommand cmd)
            return Task.CompletedTask;

        var kind = Enum.Parse<OptionKind>(cmd.Kind, ignoreCase: true);
        var aggregate = ServiceOptionAggregate.Create(
            new ServiceOptionId(cmd.ServiceOptionId),
            new ServiceDefinitionRef(cmd.ServiceDefinitionId),
            new OptionCode(cmd.Code),
            new OptionName(cmd.Name),
            kind);
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
