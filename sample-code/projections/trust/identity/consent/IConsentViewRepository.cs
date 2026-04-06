namespace Whycespace.Projections.Trust.Identity.Consent;

public interface IConsentViewRepository
{
    Task SaveAsync(ConsentReadModel model, CancellationToken ct = default);
    Task<ConsentReadModel?> GetAsync(string id, CancellationToken ct = default);
}
