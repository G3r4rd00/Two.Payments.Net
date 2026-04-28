using System;
using System.Globalization;
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

            NormalizeOrderRequest(request);

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

        private static void NormalizeOrderRequest(CreateOrderRequest request)
        {
            NormalizeBillingAddress(request);
            NormalizeShippingAddress(request);

            if (request.LineItems == null)
            {
                return;
            }

            foreach (var lineItem in request.LineItems)
            {
                if (lineItem == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(lineItem.Description) && !string.IsNullOrWhiteSpace(lineItem.Name))
                {
                    lineItem.Description = lineItem.Name;
                }

                if (string.IsNullOrWhiteSpace(lineItem.Name) && !string.IsNullOrWhiteSpace(lineItem.Description))
                {
                    lineItem.Name = lineItem.Description;
                }

                if (string.IsNullOrWhiteSpace(lineItem.QuantityUnit))
                {
                    lineItem.QuantityUnit = "pcs";
                }

                PopulateLineItemAmounts(lineItem);
            }
        }

        private static void NormalizeShippingAddress(CreateOrderRequest request)
        {
            if (request.ShippingAddress == null && request.BillingAddress != null)
            {
                request.ShippingAddress = new BillingAddress
                {
                    OrganizationName = request.BillingAddress.OrganizationName,
                    StreetAddress = request.BillingAddress.StreetAddress,
                    PostalCode = request.BillingAddress.PostalCode,
                    City = request.BillingAddress.City,
                    Country = request.BillingAddress.Country
                };
            }
        }

        private static void NormalizeBillingAddress(CreateOrderRequest request)
        {
            if (request.BillingAddress == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(request.BillingAddress.OrganizationName))
            {
                request.BillingAddress.OrganizationName = request.Buyer?.Company?.CompanyName;
            }

            if (string.IsNullOrWhiteSpace(request.BillingAddress.StreetAddress) && !string.IsNullOrWhiteSpace(request.BillingAddress.Address))
            {
                request.BillingAddress.StreetAddress = request.BillingAddress.Address;
            }

            if (string.IsNullOrWhiteSpace(request.BillingAddress.Country) && !string.IsNullOrWhiteSpace(request.BillingAddress.CountryPrefix))
            {
                request.BillingAddress.Country = request.BillingAddress.CountryPrefix;
            }
        }

        private static void PopulateLineItemAmounts(LineItem lineItem)
        {
            if (!string.IsNullOrWhiteSpace(lineItem.GrossAmount)
                && !string.IsNullOrWhiteSpace(lineItem.NetAmount)
                && !string.IsNullOrWhiteSpace(lineItem.TaxAmount))
            {
                return;
            }

            if (!TryParseDecimal(lineItem.UnitPrice, out var unitPrice))
            {
                return;
            }

            var quantity = lineItem.Quantity <= 0 ? 1 : lineItem.Quantity;
            var discountAmount = TryParseDecimal(lineItem.DiscountAmount, out var parsedDiscount)
                ? parsedDiscount
                : 0m;
            var taxRate = TryParseDecimal(lineItem.TaxRate, out var parsedTaxRate)
                ? parsedTaxRate
                : 0m;

            var netAmount = RoundCurrency((unitPrice * quantity) - discountAmount);
            var taxAmount = RoundCurrency(netAmount * taxRate);
            var grossAmount = RoundCurrency(netAmount + taxAmount);

            if (string.IsNullOrWhiteSpace(lineItem.NetAmount))
            {
                lineItem.NetAmount = FormatAmount(netAmount);
            }

            if (string.IsNullOrWhiteSpace(lineItem.TaxAmount))
            {
                lineItem.TaxAmount = FormatAmount(taxAmount);
            }

            if (string.IsNullOrWhiteSpace(lineItem.GrossAmount))
            {
                lineItem.GrossAmount = FormatAmount(grossAmount);
            }
        }

        private static bool TryParseDecimal(string value, out decimal parsed)
        {
            return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out parsed);
        }

        private static decimal RoundCurrency(decimal amount)
        {
            return decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        }

        private static string FormatAmount(decimal amount)
        {
            return amount.ToString("0.00", CultureInfo.InvariantCulture);
        }
    }
}
