using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Trading.Application.Interfaces;
using Trading.Contracts;
using Trading.Domain.Entities;
using Trading.Infrastructure.Configuration;
using Trading.Infrastructure.Networking;

namespace Trading.Application.Services
{
    /// <summary>
    /// Concrete implementation of <see cref="IOrderService"/>.  Dispatches
    /// orders to the corresponding <see cref="TcpVenueClient"/> and waits
    /// for the acknowledgement.  The caller must ensure the price service
    /// created the same venue clients.
    /// </summary>
    public class OrderService : IOrderService
    {
        private readonly Dictionary<string, TcpVenueClient> _clients;

        public OrderService(Dictionary<string, TcpVenueClient> clients)
        {
            _clients = clients;
        }

        public async Task<Order> PlaceOrderAsync(Order order, CancellationToken cancellationToken)
        {
            if (!_clients.TryGetValue(order.Venue, out var client))
            {
                throw new InvalidOperationException($"Unknown venue {order.Venue}");
            }
            var dto = new OrderNewV1
            {
                OrderId = order.Id,
                ProductId = order.ProductId,
                Side = order.Side.ToString(),
                Price = order.Price,
                Quantity = order.Quantity,
                TsUtc = DateTime.UtcNow
            };
            await client.SendOrderAsync(dto, cancellationToken).ConfigureAwait(false);
            // Wait for ack by reading the receive stream until we find matching ack.
            await foreach (var msg in client.ReceiveAsync(cancellationToken))
            {
                if (msg is OrderAckV1 ack && ack.OrderId == order.Id)
                {
                    order.ApplyAck(ack.Accepted, ack.MatchedPrice, ack.Reason);
                    return order;
                }
            }
            return order;
        }
    }
}