using Whycespace.Domain.PlatformSystem.Schema.Versioning;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Platform.Schema.Versioning;

namespace Whycespace.Engines.T2E.Platform.Schema.Versioning;

public sealed class RegisterVersioningRuleHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RegisterVersioningRuleCommand cmd)
            return Task.CompletedTask;

        var evolutionClass = cmd.EvolutionClass switch
        {
            "Breaking" => EvolutionClass.Breaking,
            "Incompatible" => EvolutionClass.Incompatible,
            _ => EvolutionClass.NonBreaking
        };

        var changes = cmd.ChangeSummary.Select(c =>
        {
            var changeType = c.ChangeType switch
            {
                "FieldRemoved" => SchemaChangeType.FieldRemoved,
                "FieldTypeChanged" => SchemaChangeType.FieldTypeChanged,
                "FieldRequiredChanged" => SchemaChangeType.FieldRequiredChanged,
                "FieldRenamed" => SchemaChangeType.FieldRenamed,
                _ => SchemaChangeType.FieldAdded
            };
            var impact = c.Impact switch
            {
                "RequiresConsumerUpdate" => ChangeImpact.RequiresConsumerUpdate,
                "Breaking" => ChangeImpact.Breaking,
                _ => ChangeImpact.Safe
            };
            return new SchemaChange(changeType, c.FieldName, impact);
        }).ToList();

        var aggregate = VersioningRuleAggregate.Register(
            new VersioningRuleId(cmd.VersioningRuleId),
            cmd.SchemaRef,
            cmd.FromVersion,
            cmd.ToVersion,
            evolutionClass,
            changes,
            new Timestamp(cmd.RegisteredAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
