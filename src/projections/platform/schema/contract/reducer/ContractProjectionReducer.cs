using Whycespace.Shared.Contracts.Events.Platform.Schema.Contract;
using Whycespace.Shared.Contracts.Platform.Schema.Contract;

namespace Whycespace.Projections.Platform.Schema.Contract.Reducer;

public static class ContractProjectionReducer
{
    public static ContractReadModel Apply(ContractReadModel state, ContractRegisteredEventSchema e, DateTimeOffset at) =>
        state with
        {
            ContractId = e.AggregateId,
            ContractName = e.ContractName,
            PublisherClassification = e.PublisherClassification,
            PublisherContext = e.PublisherContext,
            PublisherDomain = e.PublisherDomain,
            SchemaRef = e.SchemaRef,
            SchemaVersion = e.SchemaVersion,
            SubscriberCount = 0,
            Status = "Active",
            LastModifiedAt = at
        };

    public static ContractReadModel Apply(ContractReadModel state, ContractSubscriberAddedEventSchema e, DateTimeOffset at) =>
        state with { SubscriberCount = state.SubscriberCount + 1, LastModifiedAt = at };

    public static ContractReadModel Apply(ContractReadModel state, ContractDeprecatedEventSchema e, DateTimeOffset at) =>
        state with { Status = "Deprecated", LastModifiedAt = at };
}
