using System;
using Trading.Domain.Entities;

namespace Trading.Client.Wpf.ViewModels
{
    /// <summary>
    /// View model representing an order for display in the blotter.  It wraps
    /// the domain <see cref="Order"/> and exposes properties for binding.
    /// </summary>
    public class OrderViewModel : ViewModelBase
    {
        private readonly Order _order;

        public OrderViewModel(Order order)
        {
            _order = order;
        }

        public string Id => _order.Id;
        public string ProductId => _order.ProductId;
        public string Venue => _order.Venue;
        public OrderSide Side => _order.Side;
        public decimal Price => _order.Price;
        public int Quantity => _order.Quantity;
        public DateTime CreatedUtc => _order.CreatedUtc;

        public OrderStatus Status
        {
            get => _order.Status;
            private set
            {
                if (_order.Status != value)
                {
                    // There is no public setter on Order.Status; apply via ack update
                    OnPropertyChanged();
                }
            }
        }

        public decimal? MatchedPrice => _order.MatchedPrice;
        public string? RejectReason => _order.RejectReason;

        /// <summary>
        /// Refresh properties after the order has been acknowledged.  Raising
        /// notifications ensures the UI updates the status and matched price.
        /// </summary>
        public void Refresh()
        {
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(MatchedPrice));
            OnPropertyChanged(nameof(RejectReason));
        }
    }
}