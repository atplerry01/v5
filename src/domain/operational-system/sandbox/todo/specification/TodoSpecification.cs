namespace Whycespace.Domain.OperationalSystem.Sandbox.Todo;

public sealed class CanReviseTitleSpecification
{
    public bool IsSatisfiedBy(bool isCompleted)
    {
        return !isCompleted;
    }
}

public sealed class CanCompleteSpecification
{
    public bool IsSatisfiedBy(bool isCompleted)
    {
        return !isCompleted;
    }
}
