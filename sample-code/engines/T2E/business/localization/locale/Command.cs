namespace Whycespace.Engines.T2E.Business.Localization.Locale;

public record LocaleCommand(
    string Action,
    string EntityId,
    object Payload
);
