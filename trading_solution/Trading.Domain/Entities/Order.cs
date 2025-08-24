using System;

namespace Trading.Domain.Entities
{
    /// <summary>
    /// Represents an order placed by the trader against a venue.  This is a domain
    /// entity used by the application layer; it is separate from the wire
    /// contracts defined in Trading.Contracts.
    /// </summary>
    public class Order
    {
        public Order(string id, string productId, OrderSide side, decimal price, int quantity, string venue)
        {
            Id = id;
            ProductId = productId;
            Side = side;
            Price = price;
            Quantity = quantity;
            Venue = venue;
            CreatedUtc = DateTime.UtcNow;
            Status = OrderStatus.Pending;
        }

        /// <summary>
        /// Unique identifier of the order.
        /// </summary>
        public string Id { get; }

        public string ProductId { get; }

        public OrderSide Side { get; }

        public decimal Price { get; }

        public int Quantity { get; }

        /// <summary>
        /// Venue to which the order was sent.
        /// </summary>
        public string Venue { get; }

        public DateTime CreatedUtc { get; }

        public OrderStatus Status { get; private set; }

        public decimal? MatchedPrice { get; private set; }
        public string? RejectReason { get; private set; }

        /// <summary>
        /// Apply acknowledgement from venue.  Updates order state accordingly.
        /// </summary>
        public void ApplyAck(bool accepted, decimal? matchedPrice, string? reason)
        {
            if (accepted)
            {
                Status = OrderStatus.Accepted;
                MatchedPrice = matchedPrice;
                RejectReason = null;
            }
            else
            {
                Status = OrderStatus.Rejected;
                MatchedPrice = null;
                RejectReason = reason;
            }
        }
    }
}