namespace Whycespace.Projections.Constitutional.Policy.Constraint;

public interface IConstraintViewRepository
{
    Task SaveAsync(ConstraintReadModel model, CancellationToken ct = default);
    Task<ConstraintReadModel?> GetAsync(string id, CancellationToken ct = default);
}
