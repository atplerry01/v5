namespace Whycespace.Projections.Intelligence.Capacity.Constraint;

public interface IConstraintViewRepository
{
    Task SaveAsync(ConstraintReadModel model, CancellationToken ct = default);
    Task<ConstraintReadModel?> GetAsync(string id, CancellationToken ct = default);
}
