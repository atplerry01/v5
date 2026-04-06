namespace Whycespace.Domain.SharedKernel.Primitives.Kernel;

/// <summary>
/// Specifies compatibility rules for event schema evolution.
/// Backward: new optional fields allowed. Forward: unknown fields ignored.
/// Breaking changes require a version increment.
/// </summary>
public sealed class EventVersionCompatibilitySpecification : Specification<EventSchemaTransition>
{
    public override bool IsSatisfiedBy(EventSchemaTransition transition)
    {
        if (transition.From.Version >= transition.To.Version)
            return false;

        if (transition.HasRemovedRequiredFields)
            return false;

        if (transition.HasChangedFieldTypes)
            return false;

        if (transition.HasRenamedFields && !transition.IsVersionIncremented)
            return false;

        return true;
    }
}

/// <summary>
/// Represents a transition between two event schema versions for compatibility checking.
/// </summary>
public sealed record EventSchemaTransition
{
    public required EventSchemaVersion From { get; init; }
    public required EventSchemaVersion To { get; init; }

    /// <summary>True when a required field present in From is absent in To.</summary>
    public bool HasRemovedRequiredFields { get; init; }

    /// <summary>True when a field's CLR type changed between versions.</summary>
    public bool HasChangedFieldTypes { get; init; }

    /// <summary>True when a field was renamed (not added/removed) between versions.</summary>
    public bool HasRenamedFields { get; init; }

    /// <summary>True when To.Version > From.Version (major bump).</summary>
    public bool IsVersionIncremented => To.Version > From.Version;
}

/// <summary>
/// Describes a specific version of an event schema.
/// </summary>
public sealed record EventSchemaVersion
{
    public required string SchemaId { get; init; }
    public required int Version { get; init; }
    public required IReadOnlyList<EventFieldDescriptor> Fields { get; init; }
}

/// <summary>
/// Describes a single field within an event schema version.
/// </summary>
public sealed record EventFieldDescriptor
{
    public required string Name { get; init; }
    public required string TypeName { get; init; }
    public bool IsRequired { get; init; } = true;
}

/// <summary>
/// Specification that validates backward compatibility only:
/// new optional fields are allowed, nothing else changes.
/// </summary>
public sealed class BackwardCompatibilitySpecification : Specification<EventSchemaTransition>
{
    public override bool IsSatisfiedBy(EventSchemaTransition transition)
    {
        if (transition.HasRemovedRequiredFields)
            return false;

        if (transition.HasChangedFieldTypes)
            return false;

        if (transition.HasRenamedFields)
            return false;

        return true;
    }
}

/// <summary>
/// Specification that validates a breaking change is properly versioned.
/// </summary>
public sealed class BreakingChangeRequiresVersionIncrementSpecification : Specification<EventSchemaTransition>
{
    public override bool IsSatisfiedBy(EventSchemaTransition transition)
    {
        bool isBreaking = transition.HasRemovedRequiredFields
                       || transition.HasChangedFieldTypes
                       || transition.HasRenamedFields;

        return !isBreaking || transition.IsVersionIncremented;
    }
}