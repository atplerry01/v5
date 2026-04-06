namespace Whycespace.Projections.Business.Marketplace.ParticipantMarket;

public interface IParticipantMarketViewRepository
{
    Task SaveAsync(ParticipantMarketReadModel model, CancellationToken ct = default);
    Task<ParticipantMarketReadModel?> GetAsync(string id, CancellationToken ct = default);
}
