using Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Configuration;
using Whycespace.Shared.Contracts.Business.Offering.CommercialShape.Configuration;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Offering.CommercialShape.Configuration;

public sealed class CreateConfigurationHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateConfigurationCommand cmd)
            return Task.CompletedTask;

        var aggregate = ConfigurationAggregate.Create(
            new ConfigurationId(cmd.ConfigurationId),
            new ConfigurationName(cmd.Name));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
