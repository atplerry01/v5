using Whycespace.Shared.Contracts.Business.Offering.CommercialShape.Configuration;
using Whycespace.Shared.Contracts.Events.Business.Offering.CommercialShape.Configuration;

namespace Whycespace.Projections.Business.Offering.CommercialShape.Configuration.Reducer;

public static class ConfigurationProjectionReducer
{
    public static ConfigurationReadModel Apply(ConfigurationReadModel state, ConfigurationCreatedEventSchema e) =>
        state with
        {
            ConfigurationId = e.AggregateId,
            Name = e.Name,
            Status = "Draft",
            Options = state.Options
        };

    public static ConfigurationReadModel Apply(ConfigurationReadModel state, ConfigurationOptionSetEventSchema e) =>
        state with
        {
            ConfigurationId = e.AggregateId,
            Options = state.Options
                .Where(o => o.Key != e.Key)
                .Concat(new[] { new ConfigurationOptionReadModel(e.Key, e.Value) })
                .ToList()
        };

    public static ConfigurationReadModel Apply(ConfigurationReadModel state, ConfigurationOptionRemovedEventSchema e) =>
        state with
        {
            ConfigurationId = e.AggregateId,
            Options = state.Options
                .Where(o => o.Key != e.Key)
                .ToList()
        };

    public static ConfigurationReadModel Apply(ConfigurationReadModel state, ConfigurationActivatedEventSchema e) =>
        state with
        {
            ConfigurationId = e.AggregateId,
            Status = "Active"
        };

    public static ConfigurationReadModel Apply(ConfigurationReadModel state, ConfigurationArchivedEventSchema e) =>
        state with
        {
            ConfigurationId = e.AggregateId,
            Status = "Archived"
        };
}
