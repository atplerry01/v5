namespace Whycespace.Shared.Exceptions;

public sealed class ValidationException : WhyceException
{
    public IReadOnlyList<string> Errors { get; }

    public ValidationException(IEnumerable<string> errors)
        : base("One or more validation errors occurred.", "VALIDATION_ERROR")
    {
        Errors = errors.ToList().AsReadOnly();
    }

    public ValidationException(string error)
        : this(new[] { error })
    {
    }
}
