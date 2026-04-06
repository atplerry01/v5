namespace Whycespace.Shared.Contracts.Engine;

/// <summary>
/// Validation gate interface — T0U implements, T2E calls.
/// The runtime wires T0U validators into the T2E execution pipeline.
/// This ensures decision logic (T0U) runs before execution logic (T2E)
/// without creating a direct dependency between engine tiers.
/// </summary>
public interface IValidationGate
{
    Task<ValidationResult> ValidateAsync(string commandType, string entityId, CancellationToken cancellationToken = default);
}

public sealed record ValidationResult
{
    public required bool IsValid { get; init; }
    public string? Reason { get; init; }

    public static ValidationResult Valid() => new() { IsValid = true };
    public static ValidationResult Invalid(string reason) => new() { IsValid = false, Reason = reason };
}

/// <summary>
/// Default pass-through validation gate for testing and environments
/// where T0U validation is not wired up.
/// </summary>
public sealed class PassThroughValidationGate : IValidationGate
{
    public static readonly PassThroughValidationGate Instance = new();

    public Task<ValidationResult> ValidateAsync(string commandType, string entityId, CancellationToken cancellationToken = default)
        => Task.FromResult(ValidationResult.Valid());
}
