using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Trading.Contracts;

namespace Venue_C
{
    public class Program
    {
        private static readonly Random _rng = new();
        private static readonly TimeSpan UpdateInterval = TimeSpan.FromMilliseconds(500);
        private static readonly TimeSpan HeartbeatInterval = TimeSpan.FromSeconds(5);
        private static readonly string VenueName = "Venue_C";
        private static readonly List<string> Products = new() { "EURIBOR-3M", "EURIBOR-6M" };
        private static readonly Dictionary<string, decimal> MidPrices = new();
        private static readonly ConcurrentDictionary<TcpClient, NetworkStream> Clients = new();

        public static async Task Main(string[] args)
        {
            int port = 5003;
            if (args.Length > 0 && int.TryParse(args[0], out var p)) port = p;
            foreach (var prod in Products) MidPrices[prod] = 2.0m;
            var listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine($"{VenueName} listening on {port}");
            _ = Task.Run(() => AcceptLoopAsync(listener));
            _ = Task.Run(() => BroadcastLoopAsync());
            _ = Task.Run(() => HeartbeatLoopAsync());
            await Task.Delay(Timeout.Infinite);
        }

        private static async Task AcceptLoopAsync(TcpListener listener)
        {
            while (true)
            {
                var client = await listener.AcceptTcpClientAsync();
                var stream = client.GetStream();
                Clients[client] = stream;
                _ = Task.Run(() => HandleClientAsync(client, stream));
            }
        }

        private static async Task HandleClientAsync(TcpClient client, NetworkStream stream)
        {
            try
            {
                var framed = new Trading.Infrastructure.Networking.LengthPrefixedStream(stream);
                while (true)
                {
                    var doc = await framed.ReadMessageAsync(CancellationToken.None);
                    if (doc == null) break;
                    if (!doc.RootElement.TryGetProperty("schema", out var schemaProp)) continue;
                    var schema = schemaProp.GetString();
                    if (schema == "OrderNewV1")
                    {
                        var order = doc.RootElement.Deserialize<OrderNewV1>();
                        if (order != null)
                        {
                            await ProcessOrderAsync(order, framed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client error: {ex.Message}");
            }
            finally
            {
                Clients.TryRemove(client, out _);
                client.Close();
            }
        }

        private static async Task ProcessOrderAsync(OrderNewV1 order, Trading.Infrastructure.Networking.LengthPrefixedStream framed)
        {
            if (!MidPrices.TryGetValue(order.ProductId, out var mid)) return;
            decimal spread = 0.01m;
            decimal bid = mid - spread / 2;
            decimal ask = mid + spread / 2;
            bool accepted = false;
            decimal? matched = null;
            string? reason = null;
            if (order.Side == "Buy")
            {
                if (order.Price >= ask)
                {
                    accepted = true;
                    matched = ask;
                }
                else
                {
                    reason = "PriceMoved";
                }
            }
            else if (order.Side == "Sell")
            {
                if (order.Price <= bid)
                {
                    accepted = true;
                    matched = bid;
                }
                else
                {
                    reason = "PriceMoved";
                }
            }
            var ack = new OrderAckV1
            {
                OrderId = order.OrderId,
                Accepted = accepted,
                MatchedPrice = matched,
                Reason = reason,
                TsUtc = DateTime.UtcNow
            };
            await framed.WriteMessageAsync(ack, CancellationToken.None);
        }

        private static async Task BroadcastLoopAsync()
        {
            while (true)
            {
                await Task.Delay(UpdateInterval);
                foreach (var prod in Products)
                {
                    var mid = MidPrices[prod];
                    var delta = ((decimal)_rng.NextDouble() - 0.5m) * 0.02m;
                    mid = Math.Clamp(mid + delta, 1.5m, 3.5m);
                    MidPrices[prod] = mid;
                    decimal spread = 0.01m;
                    decimal bid = mid - spread / 2m;
                    decimal ask = mid + spread / 2m;
                    var update = new PriceUpdateV1
                    {
                        ProductId = prod,
                        Venue = VenueName,
                        Bid = bid,
                        Ask = ask,
                        TsUtc = DateTime.UtcNow
                    };
                    await BroadcastAsync(update);
                }
            }
        }

        private static async Task BroadcastAsync<T>(T message)
        {
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var payload = JsonSerializer.SerializeToUtf8Bytes(message, options);
            Span<byte> header = stackalloc byte[4];
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(header, (uint)payload.Length);
            var snapshot = Clients.ToArray();
            foreach (var kv in snapshot)
            {
                var stream = kv.Value;
                try
                {
                    await stream.WriteAsync(header);
                    await stream.WriteAsync(payload);
                    await stream.FlushAsync();
                }
                catch
                {
                }
            }
        }

        private static async Task HeartbeatLoopAsync()
        {
            while (true)
            {
                await Task.Delay(HeartbeatInterval);
                var hb = new VenueStatusV1
                {
                    Venue = VenueName,
                    Status = "Online",
                    TsUtc = DateTime.UtcNow
                };
                await BroadcastAsync(hb);
            }
        }
    }
}