namespace Whycespace.Domain.SharedKernel.Primitives.Kernel;

public abstract class Specification<T>
{
    public abstract bool IsSatisfiedBy(T entity);
}
