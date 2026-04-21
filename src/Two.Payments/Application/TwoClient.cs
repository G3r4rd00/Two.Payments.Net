using System;
using Two.Payments.Core.Interfaces;

namespace Two.Payments.Application
{
    /// <summary>
    /// Default implementation of <see cref="ITwoClient"/>.
    /// Acts as the main entry-point for consuming the Two payment platform.
    /// </summary>
    /// <example>
    /// <code>
    /// var client = TwoClientFactory.Create(new TwoOptions
    /// {
    ///     ApiKey    = "your-api-key",
    ///     UseSandbox = true
    /// });
    ///
    /// var order = await client.Orders.CreateOrderAsync(new CreateOrderRequest { ... });
    /// Console.WriteLine(order.Status);
    /// </code>
    /// </example>
    public sealed class TwoClient : ITwoClient
    {
        /// <inheritdoc/>
        public ITwoOrderService Orders { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="TwoClient"/>.
        /// </summary>
        /// <param name="orderService">Order service implementation.</param>
        public TwoClient(ITwoOrderService orderService)
        {
            Orders = orderService ?? throw new ArgumentNullException(nameof(orderService));
        }
    }
}
