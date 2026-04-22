using Whycespace.Domain.ControlSystem.Configuration.ConfigurationScope;
using Whycespace.Shared.Contracts.Control.Configuration.ConfigurationScope;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.Configuration.ConfigurationScope;

public sealed class DeclareConfigurationScopeHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DeclareConfigurationScopeCommand cmd)
            return Task.CompletedTask;

        var aggregate = ConfigurationScopeAggregate.Declare(
            new ConfigurationScopeId(cmd.ScopeId.ToString("N").PadRight(64, '0')),
            cmd.DefinitionId,
            cmd.Classification,
            cmd.Context);

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
