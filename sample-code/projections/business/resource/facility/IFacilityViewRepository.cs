namespace Whycespace.Projections.Business.Resource.Facility;

public interface IFacilityViewRepository
{
    Task SaveAsync(FacilityReadModel model, CancellationToken ct = default);
    Task<FacilityReadModel?> GetAsync(string id, CancellationToken ct = default);
}
