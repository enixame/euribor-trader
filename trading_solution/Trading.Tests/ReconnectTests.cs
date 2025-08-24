using System.Threading.Tasks;
using Xunit;

namespace Trading.Tests
{
    public class ReconnectTests
    {
        [Fact(Skip="Reconnect logic tested manually")] // placeholder
        public Task Client_Reconnects_On_Drop()
        {
            // In a full implementation, this test would simulate a venue drop
            // and ensure the client reconnects with backoff.  This placeholder
            // is left intentionally skipped.
            return Task.CompletedTask;
        }
    }
}