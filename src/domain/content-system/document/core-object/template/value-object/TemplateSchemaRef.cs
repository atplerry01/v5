using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Template;

/// Reference to a placeholder/schema artifact carried as a bare id.
/// Avoids cross-BC type imports per domain.guard.md rule 13.
public readonly record struct TemplateSchemaRef
{
    public Guid Value { get; }

    public TemplateSchemaRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "TemplateSchemaRef cannot be empty.");
        Value = value;
    }
}
