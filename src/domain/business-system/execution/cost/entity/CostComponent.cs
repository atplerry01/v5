namespace Whycespace.Domain.BusinessSystem.Execution.Cost;

public sealed class CostComponent
{
    public Guid ComponentId { get; }
    public string Description { get; }

    public CostComponent(Guid componentId, string description)
    {
        if (componentId == Guid.Empty)
            throw new ArgumentException("ComponentId must not be empty.", nameof(componentId));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description must not be empty.", nameof(description));

        ComponentId = componentId;
        Description = description;
    }
}
