namespace Whycespace.Runtime.ControlPlane;

public class RuntimeControlPlaneException : Exception
{
    public string? ErrorCode { get; }

    public RuntimeControlPlaneException(string message)
        : base(message)
    {
    }

    public RuntimeControlPlaneException(string message, string errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public RuntimeControlPlaneException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
