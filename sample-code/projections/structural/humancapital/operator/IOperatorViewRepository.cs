namespace Whycespace.Projections.Structural.Humancapital.Operator;

public interface IOperatorViewRepository
{
    Task SaveAsync(OperatorReadModel model, CancellationToken ct = default);
    Task<OperatorReadModel?> GetAsync(string id, CancellationToken ct = default);
}
