using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Governance.Retention;

/// Bare-id reference (with discriminator) to whatever document-context
/// artifact the retention is bound to. Avoids cross-BC type imports per
/// domain.guard.md rule 13.
public readonly record struct RetentionTargetRef
{
    public Guid Value { get; }
    public RetentionTargetKind Kind { get; }

    public RetentionTargetRef(Guid value, RetentionTargetKind kind)
    {
        Guard.Against(value == Guid.Empty, "RetentionTargetRef value cannot be empty.");
        Value = value;
        Kind = kind;
    }
}
