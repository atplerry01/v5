namespace Whycespace.Domain.PlatformSystem.Routing.RouteDefinition;

public readonly record struct RouteDefinitionStatus
{
    public static readonly RouteDefinitionStatus Active = new("Active");
    public static readonly RouteDefinitionStatus Inactive = new("Inactive");
    public static readonly RouteDefinitionStatus Deprecated = new("Deprecated");

    public string Value { get; }

    private RouteDefinitionStatus(string value) => Value = value;
}
