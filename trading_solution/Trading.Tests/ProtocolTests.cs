using System;
using System.IO;
using System.Net.Sockets;
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
        public async Task LengthPrefixedRoundTrip_Works()
        {
            // use a pair of connected sockets for in‑proc testing
            using var listener = new TcpListener(System.Net.IPAddress.Loopback, 0);
            listener.Start();
            var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
            var client = new TcpClient();
            var acceptTask = listener.AcceptTcpClientAsync();
            await client.ConnectAsync("127.0.0.1", port);
            var server = await acceptTask;
            await using var clientStream = new LengthPrefixedStream(client.GetStream());
            await using var serverStream = new LengthPrefixedStream(server.GetStream());
            var msg = new PriceUpdateV1
            {
                ProductId = "EURIBOR-3M",
                Venue = "Test",
                Bid = 2.0m,
                Ask = 2.1m,
                TsUtc = DateTime.UtcNow
            };
            await clientStream.WriteMessageAsync(msg, CancellationToken.None);
            var doc = await serverStream.ReadMessageAsync(CancellationToken.None);
            Assert.NotNull(doc);
            var schema = doc!.RootElement.GetProperty("schema").GetString();
            Assert.Equal("PriceUpdateV1", schema);
            var received = doc.RootElement.Deserialize<PriceUpdateV1>();
            Assert.NotNull(received);
            Assert.Equal(msg.Bid, received!.Bid);
        }
    }
}