namespace Whycespace.Projections.Decision.Risk.Assessment;

public interface IAssessmentViewRepository
{
    Task SaveAsync(AssessmentReadModel model, CancellationToken ct = default);
    Task<AssessmentReadModel?> GetAsync(string id, CancellationToken ct = default);
}
