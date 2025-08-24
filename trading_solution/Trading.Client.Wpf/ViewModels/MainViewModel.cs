using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Trading.Application.Services;
using Trading.Domain.Entities;
using Trading.Infrastructure.Configuration;

namespace Trading.Client.Wpf.ViewModels
{
    /// <summary>
    /// Root view model for the application.  Holds the list of product tiles
    /// and the blotter of submitted orders.  Controls the live feed via
    /// Start/Stop commands.  Handles placement of orders through the
    /// OrderService.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly TradingConfig _config;
        private readonly PriceService _priceService;
        private readonly OrderService _orderService;
        private CancellationTokenSource? _cts;

        public ObservableCollection<ProductTileViewModel> Products { get; } = new();
        public ObservableCollection<OrderViewModel> Orders { get; } = new();

        private bool _isLive;
        public bool IsLive
        {
            get => _isLive;
            private set => SetProperty(ref _isLive, value);
        }

        public ICommand ToggleLiveCommand { get; }

        public MainViewModel(TradingConfig config)
        {
            _config = config;
            _priceService = new PriceService(config);
            _orderService = new OrderService(_priceService.Clients.ToDictionary(kv => kv.Key, kv => kv.Value));
            ToggleLiveCommand = new DelegateCommand(async _ => await ToggleLiveAsync());
            // Initialize product view models
            foreach (var productId in config.Products)
            {
                var vm = new ProductTileViewModel(productId, productId);
                vm.OnOrderRequested += async (p, side) => await OnOrderRequestedAsync(p, side);
                Products.Add(vm);
            }
        }

        private async Task ToggleLiveAsync()
        {
            if (!IsLive)
            {
                _cts = new CancellationTokenSource();
                IsLive = true;
                _ = Task.Run(() => ListenForPricesAsync(_cts.Token));
            }
            else
            {
                _cts?.Cancel();
                IsLive = false;
            }
        }

        private async Task ListenForPricesAsync(CancellationToken cancellationToken)
        {
            try
            {
                await foreach (var agg in _priceService.StreamQuotesAsync(cancellationToken))
                {
                    var tile = Products.FirstOrDefault(p => p.ProductId == agg.ProductId);
                    if (tile != null)
                    {
                        // marshal to UI thread
                        Application.Current.Dispatcher.Invoke(() => tile.Update(agg));
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // expected
            }
        }

        private async Task OnOrderRequestedAsync(ProductTileViewModel tile, OrderSide side)
        {
            // Determine venue based on best price; pick first winner
            var venue = side == OrderSide.Buy
                ? tile.Venues.FirstOrDefault(v => v.IsBestAsk)?.Venue
                : tile.Venues.FirstOrDefault(v => v.IsBestBid)?.Venue;
            if (venue == null) return;
            var price = side == OrderSide.Buy ? tile.BestAsk : tile.BestBid;
            if (!price.HasValue) return;
            var qty = 1_000_000; // default quantity; could be configurable
            var order = new Order(Guid.NewGuid().ToString(), tile.ProductId, side, price.Value, qty, venue);
            var ovm = new OrderViewModel(order);
            Application.Current.Dispatcher.Invoke(() => Orders.Add(ovm));
            var cts = _cts;
            if (cts == null) return;
            _ = Task.Run(async () =>
            {
                var updated = await _orderService.PlaceOrderAsync(order, cts.Token);
                Application.Current.Dispatcher.Invoke(() => ovm.Refresh());
            });
        }
    }
}