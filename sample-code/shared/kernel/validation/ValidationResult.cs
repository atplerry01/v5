namespace Whycespace.Shared.Kernel.Validation;

public sealed record ValidationResult
{
    private readonly List<string> _errors;

    public bool IsValid => _errors.Count == 0;
    public IReadOnlyList<string> Errors => _errors.AsReadOnly();

    private ValidationResult(List<string> errors) => _errors = errors;

    public static ValidationResult Success() => new([]);

    public static ValidationResult Failure(string error) => new([error]);

    public static ValidationResult Failure(IEnumerable<string> errors) => new(errors.ToList());

    public ValidationResult Merge(ValidationResult other)
    {
        var combined = new List<string>(_errors);
        combined.AddRange(other._errors);
        return new ValidationResult(combined);
    }
}
