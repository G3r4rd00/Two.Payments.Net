using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Two.Payments.Core.Interfaces;
using Two.Payments.Core.Models;
using Two.Payments.Infrastructure;

namespace Two.Payments.Application.Services
{
    /// <summary>
    /// Implements <see cref="ITwoOrderService"/> by communicating with
    /// the Two REST API through <see cref="ITwoApiTransport"/>.
    /// </summary>
    public sealed class TwoOrderService : ITwoOrderService
    {
        private readonly ITwoApiTransport _http;
        private readonly ILogger<TwoOrderService> _logger;

        /// <summary>
        /// Initializes a new instance of <see cref="TwoOrderService"/>.
        /// </summary>
        /// <param name="http">Low-level HTTP transport (injected).</param>
        /// <param name="logger">Optional logger.</param>
        internal TwoOrderService(ITwoApiTransport http, ILogger<TwoOrderService> logger = null)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<CreateOrderResponse> CreateOrderAsync(
            CreateOrderRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            _logger?.LogInformation("Creating Two order. MerchantOrderId={MerchantOrderId}",
                request.MerchantOrderId);

            var response = await _http
                .PostAsync<CreateOrderResponse>("order", request, cancellationToken)
                .ConfigureAwait(false);

            _logger?.LogInformation("Two order created. Id={OrderId} Status={Status}",
                response?.Id, response?.Status);

            return response;
        }

        /// <inheritdoc/>
        public async Task<GetOrderResponse> GetOrderAsync(
            string orderId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(orderId))
                throw new ArgumentException("orderId must not be null or empty.", nameof(orderId));

            _logger?.LogInformation("Retrieving Two order. Id={OrderId}", orderId);

            var response = await _http
                .GetAsync<GetOrderResponse>($"order/{Uri.EscapeDataString(orderId)}", cancellationToken)
                .ConfigureAwait(false);

            _logger?.LogInformation("Retrieved Two order. Id={OrderId} Status={Status}",
                response?.Id, response?.Status);

            return response;
        }

        /// <inheritdoc/>
        public async Task ConfirmOrderAsync(
            string orderId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(orderId))
                throw new ArgumentException("orderId must not be null or empty.", nameof(orderId));

            _logger?.LogInformation("Confirming Two order. Id={OrderId}", orderId);

            await _http
                .PostAsync($"order/{Uri.EscapeDataString(orderId)}/confirm", null, cancellationToken)
                .ConfigureAwait(false);

            _logger?.LogInformation("Two order confirmed. Id={OrderId}", orderId);
        }

        /// <inheritdoc/>
        public async Task CancelOrderAsync(
            string orderId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(orderId))
                throw new ArgumentException("orderId must not be null or empty.", nameof(orderId));

            _logger?.LogInformation("Cancelling Two order. Id={OrderId}", orderId);

            await _http
                .DeleteAsync($"order/{Uri.EscapeDataString(orderId)}", cancellationToken)
                .ConfigureAwait(false);

            _logger?.LogInformation("Two order cancelled. Id={OrderId}", orderId);
        }
    }
}
