namespace Whycespace.Projections.Intelligence.Knowledge.Answer;

public interface IAnswerViewRepository
{
    Task SaveAsync(AnswerReadModel model, CancellationToken ct = default);
    Task<AnswerReadModel?> GetAsync(string id, CancellationToken ct = default);
}
