using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Event.EventDefinition;

public readonly record struct EventDefinitionId
{
    public Guid Value { get; }

    public EventDefinitionId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "EventDefinitionId cannot be empty.");
        Value = value;
    }
}
