namespace Trading.Domain.Entities
{
    /// <summary>
    /// Represents a financial instrument the system trades.  Products are
    /// identified by a unique string identifier such as "EURIBOR-3M".
    /// </summary>
    public class Product
    {
        public Product(string id)
        {
            Id = id;
        }

        /// <summary>
        /// Unique identifier of the product (e.g. EURIBOR-3M).
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Optional display name for UI purposes.  Defaults to Id.
        /// </summary>
        public string DisplayName { get; init; } = string.Empty;
    }
}