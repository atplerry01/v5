namespace Whycespace.Domain.BusinessSystem.Notification.Preference;

public sealed record PreferenceDefinedEvent(PreferenceId PreferenceId, PreferenceRule Rule);
