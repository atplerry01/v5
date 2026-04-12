namespace Whycespace.Domain.BusinessSystem.Integration.Partner;

public static class PartnerErrors
{
    public static InvalidOperationException MissingId()
        => new("PartnerId is required and must not be empty.");

    public static InvalidOperationException MissingProfile()
        => new("PartnerProfile is required.");

    public static InvalidOperationException InvalidStateTransition(PartnerStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}
