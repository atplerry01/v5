namespace Whycespace.Tests.E2E.Business.Setup;

[CollectionDefinition(Name)]
public sealed class BusinessE2ECollection : ICollectionFixture<BusinessE2EFixture>
{
    public const string Name = "BusinessE2E";
}
