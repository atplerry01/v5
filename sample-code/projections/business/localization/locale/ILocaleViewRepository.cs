namespace Whycespace.Projections.Business.Localization.Locale;

public interface ILocaleViewRepository
{
    Task SaveAsync(LocaleReadModel model, CancellationToken ct = default);
    Task<LocaleReadModel?> GetAsync(string id, CancellationToken ct = default);
}
