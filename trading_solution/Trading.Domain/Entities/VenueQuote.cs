using System;

namespace Trading.Domain.Entities
{
    /// <summary>
    /// A snapshot of the bid/ask from a single venue for a product at a given instant.
    /// </summary>
    public class VenueQuote
    {
        public VenueQuote(string productId, string venue, decimal bid, decimal ask, DateTime tsUtc)
        {
            ProductId = productId;
            Venue = venue;
            Bid = bid;
            Ask = ask;
            TimestampUtc = tsUtc;
        }

        public string ProductId { get; }
        public string Venue { get; }
        public decimal Bid { get; }
        public decimal Ask { get; }
        public DateTime TimestampUtc { get; }
    }
}