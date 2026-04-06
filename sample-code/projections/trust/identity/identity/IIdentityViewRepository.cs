using Whycespace.Projections.Identity;

namespace Whycespace.Projections.Trust.Identity.Identity;

public interface IIdentityViewRepository
{
    Task SaveAsync(IdentityReadModel model, CancellationToken ct = default);
    Task<IdentityReadModel?> GetAsync(string id, CancellationToken ct = default);
}
