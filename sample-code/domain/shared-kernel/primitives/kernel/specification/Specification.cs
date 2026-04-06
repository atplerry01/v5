namespace Whycespace.Domain.SharedKernel.Primitives.Kernel;

public abstract class Specification<T>
{
    public abstract bool IsSatisfiedBy(T entity);
    public Specification<T> And(Specification<T> other) => new AndSpecification<T>(this, other);
    public Specification<T> Or(Specification<T> other) => new OrSpecification<T>(this, other);
    public Specification<T> Not() => new NotSpecification<T>(this);
}

internal sealed class AndSpecification<T>(Specification<T> left, Specification<T> right) : Specification<T>
{
    public override bool IsSatisfiedBy(T entity) => left.IsSatisfiedBy(entity) && right.IsSatisfiedBy(entity);
}

internal sealed class OrSpecification<T>(Specification<T> left, Specification<T> right) : Specification<T>
{
    public override bool IsSatisfiedBy(T entity) => left.IsSatisfiedBy(entity) || right.IsSatisfiedBy(entity);
}

internal sealed class NotSpecification<T>(Specification<T> spec) : Specification<T>
{
    public override bool IsSatisfiedBy(T entity) => !spec.IsSatisfiedBy(entity);
}
