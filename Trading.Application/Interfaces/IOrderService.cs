using System.Threading;
using System.Threading.Tasks;
using Trading.Domain.Entities;

namespace Trading.Application.Interfaces
{
    /// <summary>
    /// Abstraction over order placement.  Implementations send orders to the
    /// appropriate venue and return the updated domain order once an ack is
    /// received.
    /// </summary>
    public interface IOrderService
    {
        Task<Order> PlaceOrderAsync(Order order, CancellationToken cancellationToken);
    }
}