namespace Whycespace.Projections.Business.Localization.Translation;

public interface ITranslationViewRepository
{
    Task SaveAsync(TranslationReadModel model, CancellationToken ct = default);
    Task<TranslationReadModel?> GetAsync(string id, CancellationToken ct = default);
}
