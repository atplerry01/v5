namespace Whycespace.Domain.BusinessSystem.Notification.Delivery;

public readonly record struct DeliveryContract
{
    public Guid ChannelReference { get; }
    public string ContractName { get; }

    public DeliveryContract(Guid channelReference, string contractName)
    {
        if (channelReference == Guid.Empty)
            throw new ArgumentException("Channel reference must not be empty.", nameof(channelReference));

        if (string.IsNullOrWhiteSpace(contractName))
            throw new ArgumentException("Contract name must not be empty.", nameof(contractName));

        ChannelReference = channelReference;
        ContractName = contractName;
    }
}
