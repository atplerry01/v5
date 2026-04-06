namespace Whycespace.Projections.Business.Agreement.Validity;

public interface IValidityViewRepository
{
    Task SaveAsync(ValidityReadModel model, CancellationToken ct = default);
    Task<ValidityReadModel?> GetAsync(string id, CancellationToken ct = default);
}
