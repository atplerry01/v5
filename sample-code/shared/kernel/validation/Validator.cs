namespace Whycespace.Shared.Kernel.Validation;

public sealed class Validator
{
    private readonly List<string> _errors = [];

    public Validator Required(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            _errors.Add($"{fieldName} is required.");
        return this;
    }

    public Validator MaxLength(string? value, int max, string fieldName)
    {
        if (value is not null && value.Length > max)
            _errors.Add($"{fieldName} must not exceed {max} characters.");
        return this;
    }

    public Validator Must(bool condition, string errorMessage)
    {
        if (!condition)
            _errors.Add(errorMessage);
        return this;
    }

    public Validator Must<T>(T value, Func<T, bool> predicate, string errorMessage)
    {
        if (!predicate(value))
            _errors.Add(errorMessage);
        return this;
    }

    public ValidationResult Build() =>
        _errors.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.Failure(_errors);
}
