using Whycespace.Domain.PlatformSystem.Schema.SchemaDefinition;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Platform.Schema.SchemaDefinition;

namespace Whycespace.Engines.T2E.Platform.Schema.SchemaDefinition;

public sealed class DraftSchemaDefinitionHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DraftSchemaDefinitionCommand cmd)
            return Task.CompletedTask;

        var compatibilityMode = cmd.CompatibilityMode switch
        {
            "Backward" => SchemaCompatibilityMode.Backward,
            "Forward" => SchemaCompatibilityMode.Forward,
            "Full" => SchemaCompatibilityMode.Full,
            _ => SchemaCompatibilityMode.None
        };

        var fields = cmd.Fields.Select(f =>
        {
            var fieldType = f.FieldType switch
            {
                "Int" => FieldType.Int,
                "Long" => FieldType.Long,
                "Bool" => FieldType.Bool,
                "Float" => FieldType.Float,
                "Bytes" => FieldType.Bytes,
                "Nested" => FieldType.Nested,
                "Array" => FieldType.Array,
                "Map" => FieldType.Map,
                _ => FieldType.String
            };
            return new FieldDescriptor(f.FieldName, fieldType, f.Required, f.DefaultValue, f.Description);
        }).ToList();

        var aggregate = SchemaDefinitionAggregate.Draft(
            new SchemaDefinitionId(cmd.SchemaDefinitionId),
            new SchemaName(cmd.SchemaName),
            cmd.Version,
            fields,
            compatibilityMode,
            new Timestamp(cmd.DraftedAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
