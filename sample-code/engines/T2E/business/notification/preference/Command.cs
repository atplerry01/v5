namespace Whycespace.Engines.T2E.Business.Notification.Preference;

public record PreferenceCommand(
    string Action,
    string EntityId,
    object Payload
);
