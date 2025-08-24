using System;
using System.Threading.Tasks;
using Trading.Domain.Aggregation;
using Trading.Domain.Entities;
using Xunit;

namespace Trading.Tests
{
    public class AggregationTests
    {
        [Fact]
        public void Calculator_Computes_BestBidAsk_Correctly()
        {
            var calc = new BestBidAskCalculator(TimeSpan.FromMinutes(1));
            var q1 = new VenueQuote("EURIBOR-3M", "Venue_A", 2.30m, 2.32m, DateTime.UtcNow);
            var q2 = new VenueQuote("EURIBOR-3M", "Venue_B", 2.31m, 2.33m, DateTime.UtcNow);
            var q3 = new VenueQuote("EURIBOR-3M", "Venue_C", 2.29m, 2.31m, DateTime.UtcNow);
            var agg1 = calc.UpdateQuote(q1);
            var agg2 = calc.UpdateQuote(q2);
            var agg3 = calc.UpdateQuote(q3);
            Assert.Equal(2.31m, agg3.BestBid);
            Assert.Equal(2.31m, agg3.BestAsk);
            Assert.Contains("Venue_B", agg3.BestBidVenues);
            Assert.Contains("Venue_C", agg3.BestAskVenues);
        }
    }
}