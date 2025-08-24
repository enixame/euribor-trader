using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Trading.Contracts;

namespace Trading.Infrastructure.Networking
{
    /// <summary>
    /// Handles a single TCP connection to a venue.  It reads and writes
    /// length‑prefixed JSON messages and exposes incoming payloads via an
    /// async enumerable.  The caller is responsible for reconnecting
    /// instances of this class when they drop.
    /// </summary>
    public class TcpVenueClient : IAsyncDisposable
    {
        private readonly string _host;
        private readonly int _port;
        private readonly string _venueName;
        private TcpClient? _client;
        private LengthPrefixedStream? _framed;

        public TcpVenueClient(string venueName, string host, int port)
        {
            _venueName = venueName;
            _host = host;
            _port = port;
        }

        /// <summary>
        /// Connects to the venue.  Creates a TcpClient and wraps its stream.
        /// </summary>
        private async Task EnsureConnectedAsync(CancellationToken cancellationToken)
        {
            if (_client != null && _client.Connected)
            {
                return;
            }
            _client?.Dispose();
            _client = new TcpClient();
            await _client.ConnectAsync(_host, _port, cancellationToken).ConfigureAwait(false);
            _framed = new LengthPrefixedStream(_client.GetStream());
        }

        /// <summary>
        /// Reads messages indefinitely until cancellation or remote close.  Each
        /// yielded item is a dynamic object corresponding to a contract type
        /// (PriceUpdateV1, VenueStatusV1 or OrderAckV1).  The enumerator
        /// terminates when the connection drops.
        /// </summary>
        public async IAsyncEnumerable<object> ReceiveAsync([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);
            if (_framed is null)
            {
                yield break;
            }
            while (!cancellationToken.IsCancellationRequested)
            {
                var doc = await _framed.ReadMessageAsync(cancellationToken).ConfigureAwait(false);
                if (doc == null)
                {
                    yield break;
                }
                if (!doc.RootElement.TryGetProperty("schema", out var schemaProp))
                {
                    continue;
                }
                string schema = schemaProp.GetString() ?? string.Empty;
                try
                {
                    switch (schema)
                    {
                        case "PriceUpdateV1":
                            var price = doc.RootElement.Deserialize<PriceUpdateV1>();
                            if (price != null) yield return price;
                            break;
                        case "VenueStatusV1":
                            var status = doc.RootElement.Deserialize<VenueStatusV1>();
                            if (status != null) yield return status;
                            break;
                        case "OrderAckV1":
                            var ack = doc.RootElement.Deserialize<OrderAckV1>();
                            if (ack != null) yield return ack;
                            break;
                        default:
                            // Unknown message type; ignore
                            break;
                    }
                }
                catch (JsonException)
                {
                    // swallow invalid payloads
                }
            }
        }

        /// <summary>
        /// Sends a new order request to the venue.
        /// </summary>
        public async Task SendOrderAsync(OrderNewV1 order, CancellationToken cancellationToken)
        {
            await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);
            if (_framed is null)
            {
                throw new InvalidOperationException("Client not connected");
            }
            await _framed.WriteMessageAsync(order, cancellationToken).ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            if (_framed != null)
            {
                await _framed.DisposeAsync().ConfigureAwait(false);
            }
            _client?.Close();
            _client?.Dispose();
            _client = null;
            _framed = null;
        }
    }
}