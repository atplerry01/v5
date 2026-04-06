namespace Whycespace.Projections.Business.Localization.CurrencyFormat;

public interface ICurrencyFormatViewRepository
{
    Task SaveAsync(CurrencyFormatReadModel model, CancellationToken ct = default);
    Task<CurrencyFormatReadModel?> GetAsync(string id, CancellationToken ct = default);
}
