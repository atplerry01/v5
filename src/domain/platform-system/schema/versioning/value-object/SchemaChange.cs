using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Schema.Versioning;

public sealed record SchemaChange
{
    public SchemaChangeType ChangeType { get; }
    public string FieldName { get; }
    public ChangeImpact Impact { get; }

    public SchemaChange(SchemaChangeType changeType, string fieldName, ChangeImpact impact)
    {
        Guard.Against(string.IsNullOrWhiteSpace(fieldName), "SchemaChange requires a non-empty FieldName.");
        ChangeType = changeType;
        FieldName = fieldName;
        Impact = impact;
    }
}
