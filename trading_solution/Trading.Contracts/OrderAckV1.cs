using System;

namespace Trading.Contracts
{
    /// <summary>
    /// Acknowledgement from a venue in response to an <see cref="OrderNewV1"/>.
    /// Contains acceptance flag, matched price, and optional rejection reason.
    /// </summary>
    public record OrderAckV1
    {
        public string Schema { get; init; } = "OrderAckV1";
        public string OrderId { get; init; } = string.Empty;
        public bool Accepted { get; init; }
        /// <summary>
        /// The matched price if the order was accepted.  If the order was
        /// rejected, this field may be null.
        /// </summary>
        public decimal? MatchedPrice { get; init; }
        /// <summary>
        /// Reason for rejection.  Null if accepted.
        /// </summary>
        public string? Reason { get; init; }
        public DateTime TsUtc { get; init; }
    }
}