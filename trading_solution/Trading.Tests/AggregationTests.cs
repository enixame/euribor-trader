using System;
using Trading.Domain.Aggregation;
using Trading.Domain.Entities;
using Xunit;

namespace Trading.Tests
{
    public class AggregationTests
    {
        [Fact]
        public void BestBidAsk_ComputedCorrectly()
        {
            var calc = new BestBidAskCalculator(TimeSpan.FromMinutes(1));
            var now = DateTime.UtcNow;
            calc.UpdateQuote(new VenueQuote("EURIBOR-3M", "Venue_A", 2.1m, 2.2m, now));
            calc.UpdateQuote(new VenueQuote("EURIBOR-3M", "Venue_B", 2.2m, 2.3m, now));
            calc.UpdateQuote(new VenueQuote("EURIBOR-3M", "Venue_C", 2.0m, 2.1m, now));
            var agg = calc.ComputeForProduct("EURIBOR-3M");
            Assert.Equal(2.2m, agg.BestBid);
            Assert.Equal(2.1m, agg.BestAsk);
            Assert.Contains("Venue_B", agg.BestBidVenues);
            Assert.Contains("Venue_C", agg.BestAskVenues);
        }
    }
}