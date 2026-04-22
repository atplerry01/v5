using Whycespace.Domain.ControlSystem.Configuration.ConfigurationResolution;
using Whycespace.Shared.Contracts.Control.Configuration.ConfigurationResolution;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.Configuration.ConfigurationResolution;

public sealed class RecordConfigurationResolutionHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RecordConfigurationResolutionCommand cmd)
            return Task.CompletedTask;

        var aggregate = ConfigurationResolutionAggregate.Record(
            new ConfigurationResolutionId(cmd.ResolutionId.ToString("N").PadRight(64, '0')),
            cmd.DefinitionId,
            cmd.ScopeId,
            cmd.StateId,
            cmd.ResolvedValue,
            cmd.ResolvedAt);

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
