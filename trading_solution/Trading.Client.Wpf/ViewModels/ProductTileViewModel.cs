using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Trading.Domain.Aggregation;
using Trading.Domain.Entities;

namespace Trading.Client.Wpf.ViewModels
{
    /// <summary>
    /// Represents a product tile in the UI showing the best bid/ask and per‑venue prices.
    /// </summary>
    public class ProductTileViewModel : ViewModelBase
    {
        public string ProductId { get; }
        public string DisplayName { get; }

        private decimal? _bestBid;
        public decimal? BestBid
        {
            get => _bestBid;
            private set => SetProperty(ref _bestBid, value);
        }

        private decimal? _bestAsk;
        public decimal? BestAsk
        {
            get => _bestAsk;
            private set => SetProperty(ref _bestAsk, value);
        }

        public ObservableCollection<VenueItemViewModel> Venues { get; } = new();

        public ICommand BuyBestCommand { get; }
        public ICommand SellBestCommand { get; }

        public event Action<ProductTileViewModel, OrderSide>? OnOrderRequested;

        public ProductTileViewModel(string productId, string displayName)
        {
            ProductId = productId;
            DisplayName = string.IsNullOrEmpty(displayName) ? productId : displayName;
            BuyBestCommand = new DelegateCommand(_ => RequestOrder(OrderSide.Buy));
            SellBestCommand = new DelegateCommand(_ => RequestOrder(OrderSide.Sell));
        }

        private void RequestOrder(OrderSide side)
        {
            OnOrderRequested?.Invoke(this, side);
        }

        /// <summary>
        /// Updates this tile based on an aggregated quote.  Also highlights
        /// venues that currently hold the best bid/ask.
        /// </summary>
        public void Update(AggregatedQuote quote)
        {
            if (quote.ProductId != ProductId) return;
            BestBid = quote.BestBid;
            BestAsk = quote.BestAsk;

            // sync venues collection with incoming quotes
            foreach (var kv in quote.Venues)
            {
                var vm = Venues.FirstOrDefault(v => v.Venue == kv.Key);
                if (vm == null)
                {
                    vm = new VenueItemViewModel(kv.Key);
                    Venues.Add(vm);
                }
                vm.Update(kv.Value, quote.BestBid, quote.BestAsk);
            }
        }
    }

    /// <summary>
    /// Represents a venue's bid/ask within a product tile.  Notifies the UI of changes.
    /// </summary>
    public class VenueItemViewModel : ViewModelBase
    {
        public string Venue { get; }
        private decimal? _bid;
        public decimal? Bid
        {
            get => _bid;
            private set => SetProperty(ref _bid, value);
        }
        private decimal? _ask;
        public decimal? Ask
        {
            get => _ask;
            private set => SetProperty(ref _ask, value);
        }
        private double _ageMs;
        public double AgeMs
        {
            get => _ageMs;
            private set => SetProperty(ref _ageMs, value);
        }
        private bool _isBestBid;
        public bool IsBestBid
        {
            get => _isBestBid;
            private set => SetProperty(ref _isBestBid, value);
        }
        private bool _isBestAsk;
        public bool IsBestAsk
        {
            get => _isBestAsk;
            private set => SetProperty(ref _isBestAsk, value);
        }

        private DateTime _lastUpdateUtc;

        public VenueItemViewModel(string venue)
        {
            Venue = venue;
        }

        public void Update(VenueQuote quote, decimal? bestBid, decimal? bestAsk)
        {
            Bid = quote.Bid;
            Ask = quote.Ask;
            _lastUpdateUtc = quote.TimestampUtc;
            IsBestBid = bestBid.HasValue && quote.Bid == bestBid.Value;
            IsBestAsk = bestAsk.HasValue && quote.Ask == bestAsk.Value;
            AgeMs = (DateTime.UtcNow - _lastUpdateUtc).TotalMilliseconds;
        }
    }
}