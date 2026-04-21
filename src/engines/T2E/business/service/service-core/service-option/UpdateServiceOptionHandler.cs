using Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceOption;
using Whycespace.Shared.Contracts.Business.Service.ServiceCore.ServiceOption;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Service.ServiceCore.ServiceOption;

public sealed class UpdateServiceOptionHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not UpdateServiceOptionCommand cmd)
            return;

        var kind = Enum.Parse<OptionKind>(cmd.Kind, ignoreCase: true);
        var aggregate = (ServiceOptionAggregate)await context.LoadAggregateAsync(typeof(ServiceOptionAggregate));
        aggregate.Update(new OptionName(cmd.Name), kind);
        context.EmitEvents(aggregate.DomainEvents);
    }
}
