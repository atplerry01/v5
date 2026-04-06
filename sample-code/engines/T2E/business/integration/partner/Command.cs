namespace Whycespace.Engines.T2E.Business.Integration.Partner;

public record PartnerCommand(
    string Action,
    string EntityId,
    object Payload
);
