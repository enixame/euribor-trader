using System;
using System.IO;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Trading.Contracts;
using Trading.Infrastructure.Networking;
using Xunit;

namespace Trading.Tests
{
    public class ProtocolTests
    {
        [Fact]
        public async Task LengthPrefixedStream_Roundtrip()
        {
            // Use memory stream to simulate network stream
            using var mem = new MemoryStream();
            var framed = new LengthPrefixedStream(mem);
            var dto = new PriceUpdateV1
            {
                ProductId = "EURIBOR-3M",
                Venue = "Test",
                Bid = 2.34m,
                Ask = 2.35m,
                TsUtc = DateTime.UtcNow
            };
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            await framed.WriteMessageAsync(dto, cts.Token);
            mem.Position = 0;
            var doc = await framed.ReadMessageAsync(cts.Token);
            Assert.NotNull(doc);
            var schema = doc!.RootElement.GetProperty("schema").GetString();
            Assert.Equal("PriceUpdateV1", schema);
        }
    }
}