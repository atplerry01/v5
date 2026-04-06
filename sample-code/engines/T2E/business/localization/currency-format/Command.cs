namespace Whycespace.Engines.T2E.Business.Localization.CurrencyFormat;

public record CurrencyFormatCommand(
    string Action,
    string EntityId,
    object Payload
);
