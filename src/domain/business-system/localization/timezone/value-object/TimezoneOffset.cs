namespace Whycespace.Domain.BusinessSystem.Localization.Timezone;

public readonly record struct TimezoneOffset
{
    public string IanaId { get; }
    public int UtcOffsetMinutes { get; }

    public TimezoneOffset(string ianaId, int utcOffsetMinutes)
    {
        if (string.IsNullOrWhiteSpace(ianaId))
            throw new ArgumentException("IANA timezone identifier must not be empty.", nameof(ianaId));

        if (utcOffsetMinutes < -720 || utcOffsetMinutes > 840)
            throw new ArgumentException("UTC offset must be between -720 and +840 minutes.", nameof(utcOffsetMinutes));

        IanaId = ianaId;
        UtcOffsetMinutes = utcOffsetMinutes;
    }
}
