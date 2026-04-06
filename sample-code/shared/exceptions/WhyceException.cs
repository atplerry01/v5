namespace Whycespace.Shared.Exceptions;

public class WhyceException : Exception
{
    public string Code { get; }

    public WhyceException(string message, string code = "WHYCE_ERROR")
        : base(message)
    {
        Code = code;
    }

    public WhyceException(string message, Exception innerException, string code = "WHYCE_ERROR")
        : base(message, innerException)
    {
        Code = code;
    }
}
