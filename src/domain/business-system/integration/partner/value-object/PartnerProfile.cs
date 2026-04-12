namespace Whycespace.Domain.BusinessSystem.Integration.Partner;

public readonly record struct PartnerProfile
{
    public string PartnerName { get; }
    public string PartnerType { get; }

    public PartnerProfile(string partnerName, string partnerType)
    {
        if (string.IsNullOrWhiteSpace(partnerName))
            throw new ArgumentException("PartnerName must not be empty.", nameof(partnerName));

        if (string.IsNullOrWhiteSpace(partnerType))
            throw new ArgumentException("PartnerType must not be empty.", nameof(partnerType));

        PartnerName = partnerName;
        PartnerType = partnerType;
    }
}
