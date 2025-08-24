using System;

namespace Trading.Contracts
{
    /// <summary>
    /// Represents a price update event published by a venue.  The schema field
    /// is included to allow versioned payloads on the wire.  The client
    /// aggregates multiple <see cref="PriceUpdateV1"/> messages to compute
    /// the best bid and ask across venues.
    /// </summary>
    public record PriceUpdateV1
    {
        public string Schema { get; init; } = "PriceUpdateV1";
        public string ProductId { get; init; } = string.Empty;
        public string Venue { get; init; } = string.Empty;
        public decimal Bid { get; init; }
        public decimal Ask { get; init; }
        public DateTime TsUtc { get; init; }
    }
}