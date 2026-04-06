namespace Whycespace.Projections.Business.Subscription.Enrollment;

public interface IEnrollmentViewRepository
{
    Task SaveAsync(EnrollmentReadModel model, CancellationToken ct = default);
    Task<EnrollmentReadModel?> GetAsync(string id, CancellationToken ct = default);
}
