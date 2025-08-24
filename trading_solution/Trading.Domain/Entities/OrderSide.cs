namespace Trading.Domain.Entities
{
    /// <summary>
    /// Side of the order.  Buy means the trader wants to buy at the ask; Sell means to sell at the bid.
    /// </summary>
    public enum OrderSide
    {
        Buy,
        Sell
    }
}