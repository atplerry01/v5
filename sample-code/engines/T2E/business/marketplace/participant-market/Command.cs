namespace Whycespace.Engines.T2E.Business.Marketplace.ParticipantMarket;

public record ParticipantMarketCommand(
    string Action,
    string EntityId,
    object Payload
);
