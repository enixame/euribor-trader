namespace Trading.Infrastructure.Configuration
{
    /// <summary>
    /// Represents the TCP endpoint for a venue.  These settings are bound
    /// from configuration files via <c>IOptions</c>.
    /// </summary>
    public class VenueEndpoint
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 0;
        public int UpdateIntervalMs { get; set; } = 200;
    }
}