using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Trading.Application.Interfaces;
using Trading.Contracts;
using Trading.Domain.Aggregation;
using Trading.Infrastructure.Configuration;
using Trading.Infrastructure.Networking;

namespace Trading.Application.Services
{
    /// <summary>
    /// Concrete implementation of <see cref="IPriceService"/>.  It connects
    /// to all configured venues, aggregates their price updates via the
    /// <see cref="BestBidAskCalculator"/> and yields aggregated quotes to
    /// consumers.  When a venue reconnects or becomes stale, quotes are
    /// updated accordingly.
    /// </summary>
    public class PriceService : IPriceService
    {
        private readonly TradingConfig _config;
        private readonly BestBidAskCalculator _calculator;
        private readonly Dictionary<string, TcpVenueClient> _clients = new();

        public PriceService(TradingConfig config)
        {
            _config = config;
            _calculator = new BestBidAskCalculator(TimeSpan.FromMilliseconds(config.StaleMs));
            foreach (var kv in config.Venues)
            {
                _clients[kv.Key] = new TcpVenueClient(kv.Key, kv.Value.Host, kv.Value.Port);
            }
        }

        /// <summary>
        /// Exposes the underlying venue client instances.  OrderService uses
        /// them to send orders.  Consumers should not modify the returned
        /// dictionary.
        /// </summary>
        public IReadOnlyDictionary<string, TcpVenueClient> Clients => _clients;

        public async IAsyncEnumerable<AggregatedQuote> StreamQuotesAsync([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            // Channel used to serialise all aggregated updates to consumers.  Bounded
            // to prevent unbounded memory growth when UI cannot keep up.
            var channel = Channel.CreateUnbounded<AggregatedQuote>();

            // Launch a receiver for each venue.  Each will push aggregated quotes into the channel.
            var tasks = _clients.Select(client => Task.Run(async () =>
            {
                await foreach (var msg in client.Value.ReceiveAsync(cancellationToken))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    switch (msg)
                    {
                        case PriceUpdateV1 price:
                            var quote = new Trading.Domain.Entities.VenueQuote(price.ProductId, price.Venue, price.Bid, price.Ask, price.TsUtc);
                            var agg = _calculator.UpdateQuote(quote);
                            await channel.Writer.WriteAsync(agg, cancellationToken).ConfigureAwait(false);
                            break;
                        case VenueStatusV1 status:
                            // Currently ignore status updates.  Staleness is handled via timestamp.
                            break;
                        case OrderAckV1 ack:
                            // Order acknowledgements are handled by order service.
                            break;
                        default:
                            break;
                    }
                }
            }, cancellationToken)).ToArray();

            // Pump aggregated quotes to the consumer until cancelled.
            await foreach (var agg in channel.Reader.ReadAllAsync(cancellationToken))
            {
                yield return agg;
            }

            // Wait for all receivers to finish.
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var c in _clients.Values)
            {
                await c.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}