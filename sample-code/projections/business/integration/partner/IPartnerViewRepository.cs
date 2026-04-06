namespace Whycespace.Projections.Business.Integration.Partner;

public interface IPartnerViewRepository
{
    Task SaveAsync(PartnerReadModel model, CancellationToken ct = default);
    Task<PartnerReadModel?> GetAsync(string id, CancellationToken ct = default);
}
