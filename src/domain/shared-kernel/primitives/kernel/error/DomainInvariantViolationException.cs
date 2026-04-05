namespace Whycespace.Domain.SharedKernel.Primitives.Kernel;

public class DomainInvariantViolationException : DomainException
{
    public DomainInvariantViolationException(string message) : base(message) { }
}
