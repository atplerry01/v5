namespace Whycespace.Shared.Contracts.Infrastructure;

/// <summary>
/// Contract for event payload schema validation at the infrastructure boundary.
/// Implementations should delegate to IEventSchemaRegistry for schema resolution.
/// </summary>
public interface IEventSchemaValidator
{
    EventValidationResult Validate(string eventType, string payload);
    EventValidationResult ValidateBatch(string eventType, IEnumerable<string> payloads);
}

/// <summary>
/// Result of event schema validation.
/// </summary>
public sealed record EventValidationResult(bool IsValid, string? Error = null)
{
    public static EventValidationResult Valid() => new(true);
    public static EventValidationResult Invalid(string error) => new(false, error);
}
