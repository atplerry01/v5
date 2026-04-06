namespace Whycespace.Projections.Decision.Governance.Guardian;

public interface IGuardianViewRepository
{
    Task SaveAsync(GuardianReadModel model, CancellationToken ct = default);
    Task<GuardianReadModel?> GetAsync(string id, CancellationToken ct = default);
}
