using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Trading.Domain.Entities;

namespace Trading.Domain.Aggregation
{
    /// <summary>
    /// Maintains the latest quotes from each venue per product and computes the
    /// best bid and ask across all venues.  This type is thread-safe.
    /// </summary>
    public class BestBidAskCalculator
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, VenueQuote>> _quotes = new();

        /// <summary>
        /// Staleness threshold.  Quotes older than this many milliseconds are
        /// considered stale and ignored in the best-price computation.
        /// </summary>
        private readonly TimeSpan _staleThreshold;

        public BestBidAskCalculator(TimeSpan? staleThreshold = null)
        {
            _staleThreshold = staleThreshold ?? TimeSpan.FromSeconds(5);
        }

        /// <summary>
        /// Update or insert a quote from a venue.  Returns the computed
        /// aggregated view for the product after the update.
        /// </summary>
        public AggregatedQuote UpdateQuote(VenueQuote quote)
        {
            var venueMap = _quotes.GetOrAdd(quote.ProductId, _ => new ConcurrentDictionary<string, VenueQuote>());
            venueMap[quote.Venue] = quote;
            return ComputeForProduct(quote.ProductId);
        }

        /// <summary>
        /// Compute the aggregated best bid and ask for a given product.
        /// </summary>
        public AggregatedQuote ComputeForProduct(string productId)
        {
            if (!_quotes.TryGetValue(productId, out var venueMap))
            {
                return new AggregatedQuote(productId, null, null, new Dictionary<string, VenueQuote>());
            }

            var now = DateTime.UtcNow;
            var filtered = venueMap.Values
                .Where(q => (now - q.TimestampUtc) <= _staleThreshold)
                .ToList();

            decimal? bestBid = filtered.Count > 0 ? filtered.Max(q => q.Bid) : null;
            decimal? bestAsk = filtered.Count > 0 ? filtered.Min(q => q.Ask) : null;

            return new AggregatedQuote(productId, bestBid, bestAsk, filtered.ToDictionary(q => q.Venue, q => q));
        }
    }

    /// <summary>
    /// Represents the aggregated view across venues for a product.
    /// Contains the best bid/ask and the contributing quotes.
    /// </summary>
    public class AggregatedQuote
    {
        public AggregatedQuote(string productId, decimal? bestBid, decimal? bestAsk, IReadOnlyDictionary<string, VenueQuote> venues)
        {
            ProductId = productId;
            BestBid = bestBid;
            BestAsk = bestAsk;
            Venues = venues;
        }

        public string ProductId { get; }
        public decimal? BestBid { get; }
        public decimal? BestAsk { get; }
        public IReadOnlyDictionary<string, VenueQuote> Venues { get; }

        /// <summary>
        /// Gets the venues that currently hold the best bid.
        /// </summary>
        public IEnumerable<string> BestBidVenues =>
            BestBid.HasValue
                ? Venues.Where(kv => kv.Value.Bid == BestBid.Value).Select(kv => kv.Key)
                : Enumerable.Empty<string>();

        /// <summary>
        /// Gets the venues that currently hold the best ask.
        /// </summary>
        public IEnumerable<string> BestAskVenues =>
            BestAsk.HasValue
                ? Venues.Where(kv => kv.Value.Ask == BestAsk.Value).Select(kv => kv.Key)
                : Enumerable.Empty<string>();
    }
}