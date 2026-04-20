namespace Whycespace.Shared.Contracts.Runtime;

/// <summary>
/// Canonical validation outcome. Returned by <see cref="ISemanticValidator{TCommand}"/>
/// and <see cref="IStateTransitionValidator"/>. A valid result carries no failures;
/// an invalid result carries one or more <see cref="ValidationFailure"/> entries
/// each classified by <see cref="ValidationFailureCategory"/>.
/// </summary>
public sealed record ValidationResult
{
    public bool IsValid { get; init; }
    public IReadOnlyList<ValidationFailure> Failures { get; init; } = [];

    public static ValidationResult Valid { get; } = new() { IsValid = true };

    public static ValidationResult Invalid(params ValidationFailure[] failures) =>
        new() { IsValid = false, Failures = failures };

    public static ValidationResult Invalid(ValidationFailureCategory category, string message, string? field = null) =>
        new()
        {
            IsValid = false,
            Failures = [new ValidationFailure(category, message, field)]
        };
}

public sealed record ValidationFailure(
    ValidationFailureCategory Category,
    string Message,
    string? Field = null);
