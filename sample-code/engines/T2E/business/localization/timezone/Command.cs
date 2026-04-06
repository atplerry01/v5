namespace Whycespace.Engines.T2E.Business.Localization.Timezone;

public record TimezoneCommand(
    string Action,
    string EntityId,
    object Payload
);
