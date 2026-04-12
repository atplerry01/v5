namespace Whycespace.Domain.BusinessSystem.Localization.Timezone;

public sealed record TimezoneCreatedEvent(TimezoneId TimezoneId, TimezoneOffset Offset);
