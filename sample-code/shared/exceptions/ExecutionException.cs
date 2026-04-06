namespace Whycespace.Shared.Exceptions;

public sealed class ExecutionException : WhyceException
{
    public ExecutionException(string message)
        : base(message, "EXECUTION_ERROR")
    {
    }

    public ExecutionException(string message, Exception innerException)
        : base(message, innerException, "EXECUTION_ERROR")
    {
    }
}
