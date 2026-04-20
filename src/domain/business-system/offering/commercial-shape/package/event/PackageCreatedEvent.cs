namespace Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Package;

public sealed record PackageCreatedEvent(PackageId PackageId, PackageCode Code, PackageName Name);
