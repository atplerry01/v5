namespace Whycespace.Projections.Business.Entitlement.Eligibility;

public interface IEligibilityViewRepository
{
    Task SaveAsync(EligibilityReadModel model, CancellationToken ct = default);
    Task<EligibilityReadModel?> GetAsync(string id, CancellationToken ct = default);
}
