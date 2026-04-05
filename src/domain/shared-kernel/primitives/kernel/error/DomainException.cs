namespace Whycespace.Domain.SharedKernel.Primitives.Kernel;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
