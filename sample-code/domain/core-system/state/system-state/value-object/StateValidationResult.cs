namespace Whycespace.Domain.CoreSystem.State.SystemState;

/// <summary>
/// Result of a cross-system state validation check.
/// </summary>
public sealed record StateValidationResult
{
    public bool IsValid { get; }
    public string SystemName { get; }
    public string Details { get; }

    private StateValidationResult(bool isValid, string systemName, string details)
    {
        IsValid = isValid;
        SystemName = systemName;
        Details = details;
    }

    public static StateValidationResult Valid(string systemName) =>
        new(true, systemName, "State is coherent");

    public static StateValidationResult Invalid(string systemName, string details) =>
        new(false, systemName, details);
}
