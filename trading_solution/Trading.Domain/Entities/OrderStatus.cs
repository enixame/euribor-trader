namespace Trading.Domain.Entities
{
    /// <summary>
    /// Represents the lifecycle state of an order as maintained by the client.
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>Order has been created but not yet acknowledged by the venue.</summary>
        Pending,
        /// <summary>Order was accepted by the venue.</summary>
        Accepted,
        /// <summary>Order was rejected by the venue.</summary>
        Rejected
    }
}