namespace Whycespace.Projections.Structural.Humancapital.Sponsorship;

public interface ISponsorshipViewRepository
{
    Task SaveAsync(SponsorshipReadModel model, CancellationToken ct = default);
    Task<SponsorshipReadModel?> GetAsync(string id, CancellationToken ct = default);
}
