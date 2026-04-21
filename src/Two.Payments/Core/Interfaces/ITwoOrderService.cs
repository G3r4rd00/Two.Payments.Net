using System.Threading;
using System.Threading.Tasks;
using Two.Payments.Core.Models;

namespace Two.Payments.Core.Interfaces
{
    /// <summary>
    /// Provides order management operations against the Two API.
    /// </summary>
    public interface ITwoOrderService
    {
        /// <summary>
        /// Creates a new order (checkout session) with the Two platform.
        /// </summary>
        /// <param name="request">The order creation payload.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created order details including status and any redirect URL.</returns>
        Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the current status and details of an existing order.
        /// </summary>
        /// <param name="orderId">The Two-assigned order identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Full order details including current status.</returns>
        Task<GetOrderResponse> GetOrderAsync(string orderId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Confirms a previously created order, signalling that the merchant has fulfilled it.
        /// </summary>
        /// <param name="orderId">The Two-assigned order identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task ConfirmOrderAsync(string orderId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cancels an order that has not yet been confirmed or fulfilled.
        /// </summary>
        /// <param name="orderId">The Two-assigned order identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task CancelOrderAsync(string orderId, CancellationToken cancellationToken = default);
    }
}
