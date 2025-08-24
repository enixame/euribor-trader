using System;

namespace Trading.Contracts
{
    /// <summary>
    /// Represents the heartbeat/status message emitted by a venue.  Clients
    /// can use this to detect offline venues and adjust aggregation accordingly.
    /// </summary>
    public record VenueStatusV1
    {
        public string Schema { get; init; } = "VenueStatusV1";
        public string Venue { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public DateTime TsUtc { get; init; }
    }
}