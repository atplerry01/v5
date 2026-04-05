namespace Whycespace.Domain.SharedKernel.Primitives.Kernel;

public static class Guard
{
    public static void Against(bool condition, string message)
    {
        if (condition) throw new DomainInvariantViolationException(message);
    }
}
