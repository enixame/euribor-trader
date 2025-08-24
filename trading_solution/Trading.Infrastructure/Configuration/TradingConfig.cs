using System.Collections.Generic;

namespace Trading.Infrastructure.Configuration
{
    /// <summary>
    /// Root configuration bound from appsettings.json.  Contains lists of products and venue endpoints.
    /// </summary>
    public class TradingConfig
    {
        public List<string> Products { get; set; } = new();
        public Dictionary<string, VenueEndpoint> Venues { get; set; } = new();
        public int StaleMs { get; set; } = 5_000;
    }
}