using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Trading.Domain.Aggregation;

namespace Trading.Application.Interfaces
{
    /// <summary>
    /// Exposes a stream of aggregated quotes to subscribers.  The concrete
    /// implementation will connect to venues via the infrastructure layer,
    /// aggregate quotes using the domain calculator and publish updates.
    /// </summary>
    public interface IPriceService : IAsyncDisposable
    {
        /// <summary>
        /// Starts streaming quotes for all configured products.  Returns an
        /// async enumerable of aggregated quotes.  Cancellation token is
        /// observed to gracefully stop streaming.
        /// </summary>
        IAsyncEnumerable<AggregatedQuote> StreamQuotesAsync(CancellationToken cancellationToken);
    }
}