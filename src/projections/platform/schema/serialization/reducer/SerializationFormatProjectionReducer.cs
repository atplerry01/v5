using Whycespace.Shared.Contracts.Events.Platform.Schema.Serialization;
using Whycespace.Shared.Contracts.Platform.Schema.Serialization;

namespace Whycespace.Projections.Platform.Schema.Serialization.Reducer;

public static class SerializationFormatProjectionReducer
{
    public static SerializationFormatReadModel Apply(SerializationFormatReadModel state, SerializationFormatRegisteredEventSchema e, DateTimeOffset at) =>
        state with
        {
            SerializationFormatId = e.AggregateId,
            FormatName = e.FormatName,
            Encoding = e.Encoding,
            SchemaRef = e.SchemaRef,
            RoundTripGuarantee = e.RoundTripGuarantee,
            FormatVersion = e.FormatVersion,
            Status = "Active",
            LastModifiedAt = at
        };

    public static SerializationFormatReadModel Apply(SerializationFormatReadModel state, SerializationFormatDeprecatedEventSchema e, DateTimeOffset at) =>
        state with { Status = "Deprecated", LastModifiedAt = at };
}
