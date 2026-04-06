namespace Whycespace.Projections.Trust.Identity.Profile;

public interface IProfileViewRepository
{
    Task SaveAsync(ProfileReadModel model, CancellationToken ct = default);
    Task<ProfileReadModel?> GetAsync(string id, CancellationToken ct = default);
}
