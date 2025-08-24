using System;

namespace Trading.Contracts
{
    /// <summary>
    /// Represents a new order request sent by the client to a venue.  The
    /// venue evaluates the order against its current book and replies with
    /// an <see cref="OrderAckV1"/>.
    /// </summary>
    public record OrderNewV1
    {
        public string Schema { get; init; } = "OrderNewV1";
        public string OrderId { get; init; } = string.Empty;
        public string ProductId { get; init; } = string.Empty;
        /// <summary>
        /// Side of the order: Buy or Sell.  Use string to remain payload
        /// compatible; map to domain enum at higher layers.
        /// </summary>
        public string Side { get; init; } = string.Empty;
        public decimal Price { get; init; }
        public int Quantity { get; init; }
        public DateTime TsUtc { get; init; }
        public override string ToString()
        {
            return $"{Side} {Quantity} {ProductId} @ {Price}";
        }
    }
}